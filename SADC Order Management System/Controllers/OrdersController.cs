using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SADC_Order_Management_System.Authorization;
using SADC_Order_Management_System.DTOs.Requests;
using SADC_Order_Management_System.DTOs.Responses;
using SADC_Order_Management_System.Helpers;
using SADC_Order_Management_System.Services.Interfaces;

namespace SADC_Order_Management_System.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [Authorize(Policy = PolicyNames.OrdersWrite)]
        public async Task<ActionResult<OrderResponseDto>> Create([FromBody] CreateOrderRequestDto dto)
        {
            var correlationId = HttpContext.Items[CorrelationHelper.HeaderName]?.ToString();
            var response = await _orderService.CreateAsync(dto, correlationId);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = PolicyNames.OrdersRead)]
        public async Task<ActionResult<OrderResponseDto>> GetById(Guid id)
        {
            var response = await _orderService.GetByIdAsync(id);
            if (response == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(response.ETag))
            {
                Response.Headers.ETag = $"\"{response.ETag}\"";
            }

            return Ok(response);
        }

        [HttpGet]
        [Authorize(Policy = PolicyNames.OrdersRead)]
        public async Task<ActionResult<PagedResponseDto<OrderResponseDto>>> GetPaged(
            [FromQuery] Guid? customerId,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sort = "createdAt_desc")
        {
            return Ok(await _orderService.GetPagedAsync(customerId, status, page, pageSize, sort));
        }

        [HttpPut("{id:guid}/status")]
        [Authorize(Policy = PolicyNames.OrdersAdmin)]
        public async Task<ActionResult<OrderResponseDto>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequestDto dto)
        {
            var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return BadRequest("Idempotency-Key header is required.");
            }

            var response = await _orderService.UpdateStatusAsync(id, dto, idempotencyKey);
            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }
    }
}