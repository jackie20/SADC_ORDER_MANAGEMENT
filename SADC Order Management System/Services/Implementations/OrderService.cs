using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SADC_Order_Management_System.DTOs.Requests;
using SADC_Order_Management_System.DTOs.Responses;
using SADC_Order_Management_System.Helpers;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Interfaces;
using SADC_Order_Management_System.Services.Interfaces;

namespace SADC_Order_Management_System.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IOutboxRepository outboxRepository,
            IIdempotencyService idempotencyService,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _outboxRepository = outboxRepository;
            _idempotencyService = idempotencyService;
            _logger = logger;
        }

        public async Task<OrderResponseDto> CreateAsync(CreateOrderRequestDto dto, string? correlationId)
        {
            var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
            if (customer == null)
            {
                throw new KeyNotFoundException("Customer not found.");
            }

            var currencyCode = dto.CurrencyCode.Trim().ToUpperInvariant();
            if (!CurrencyHelper.IsValidCountryCurrencyPair(customer.CountryCode, currencyCode))
            {
                throw new InvalidOperationException($"Invalid country/currency pairing. Country={customer.CountryCode}, Currency={currencyCode}");
            }

            if (dto.LineItems == null || dto.LineItems.Count == 0)
            {
                throw new InvalidOperationException("Order must contain at least one line item.");
            }

            foreach (var line in dto.LineItems)
            {
                if (line.Quantity <= 0)
                {
                    throw new InvalidOperationException("Quantity must be greater than zero.");
                }

                if (line.UnitPrice < 0)
                {
                    throw new InvalidOperationException("UnitPrice must be zero or greater.");
                }
            }

            var order = new Order
            {
                CustomerId = dto.CustomerId,
                CurrencyCode = currencyCode,
                OrderStatus = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                LineItems = dto.LineItems.Select(x => new OrderLineItem
                {
                    ProductSku = x.ProductSku.Trim(),
                    Quantity = x.Quantity,
                    UnitPrice = FxRoundingHelper.RoundMoney(x.UnitPrice),
                    CreatedAt = DateTime.UtcNow
                }).ToList()
            };

            order.TotalAmount = FxRoundingHelper.RoundMoney(order.LineItems.Sum(x => x.Quantity * x.UnitPrice));

            await using var tx = await _orderRepository.BeginTransactionAsync();
            await _orderRepository.CreateAsync(order);

            var outbox = new OutboxMessage
            {
                AggregateType = "Order",
                AggregateId = order.Id,
                Type = "OrderCreated",
                Payload = JsonSerializer.Serialize(new
                {
                    MessageId = Guid.NewGuid(),
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,
                    CurrencyCode = order.CurrencyCode,
                    TotalAmount = order.TotalAmount,
                    OccurredAtUtc = DateTime.UtcNow,
                    Version = 1
                }),
                CorrelationId = correlationId,
                OccurredAtUtc = DateTime.UtcNow,
                Version = 1
            };

            await _outboxRepository.AddAsync(outbox);
            await _outboxRepository.SaveChangesAsync();
            await tx.CommitAsync();

            _logger.LogInformation("Order created successfully. OrderId={OrderId}", order.Id);

            return MapOrder(order);
        }

        public async Task<OrderResponseDto?> GetByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order == null ? null : MapOrder(order);
        }

        public async Task<PagedResponseDto<OrderResponseDto>> GetPagedAsync(Guid? customerId, string? status, int page, int pageSize, string sort)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

            var (items, total) = await _orderRepository.GetPagedAsync(customerId, status, page, pageSize, sort);

            return new PagedResponseDto<OrderResponseDto>
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Data = items.Select(MapOrder).ToList()
            };
        }

        public async Task<OrderResponseDto?> UpdateStatusAsync(Guid id, UpdateOrderStatusRequestDto dto, string idempotencyKey)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                throw new InvalidOperationException("Idempotency-Key header is required.");
            }

            var existingRecord = await _idempotencyService.GetAsync(idempotencyKey);
            if (existingRecord != null)
            {
                var cached = JsonSerializer.Deserialize<OrderResponseDto>(existingRecord.ResponseBody);
                return cached;
            }

            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return null;
            }

            if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var nextStatus))
            {
                throw new InvalidOperationException("Invalid status value.");
            }

            if (!CanTransition(order.OrderStatus, nextStatus))
            {
                throw new InvalidOperationException($"Invalid status transition from {order.OrderStatus} to {nextStatus}.");
            }

            if (order.OrderStatus != nextStatus)
            {
                order.OrderStatus = nextStatus;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);
            }

            var response = MapOrder(order);
            await _idempotencyService.SaveAsync(idempotencyKey, $"/api/orders/{id}/status", "PUT", 200, JsonSerializer.Serialize(response));
            return response;
        }

        private static bool CanTransition(OrderStatus current, OrderStatus next)
        {
            if (current == next) return true;

            return current switch
            {
                OrderStatus.Pending => next == OrderStatus.Paid || next == OrderStatus.Cancelled,
                OrderStatus.Paid => next == OrderStatus.Fulfilled || next == OrderStatus.Cancelled,
                OrderStatus.Fulfilled => false,
                OrderStatus.Cancelled => false,
                _ => false
            };
        }

        private static OrderResponseDto MapOrder(Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = order.OrderStatus.ToString(),
                CreatedAt = order.CreatedAt,
                CurrencyCode = order.CurrencyCode,
                TotalAmount = order.TotalAmount,
                ETag = order.RowVersion?.Length > 0 ? ETagHelper.FromRowVersion(order.RowVersion) : string.Empty,
                LineItems = order.LineItems.Select(li => new OrderLineItemResponseDto
                {
                    Id = li.Id,
                    ProductSku = li.ProductSku,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice,
                    LineTotal = FxRoundingHelper.RoundMoney(li.Quantity * li.UnitPrice)
                }).ToList()
            };
        }
    }
}