using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SADC_Order_Management_System.Authorization;
using SADC_Order_Management_System.DTOs.Requests;
using SADC_Order_Management_System.DTOs.Responses;
using SADC_Order_Management_System.Services.Interfaces;

namespace SADC_Order_Management_System.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        [Authorize(Policy = PolicyNames.OrdersWrite)]
        public async Task<ActionResult<CustomerResponseDto>> Create([FromBody] CreateCustomerRequestDto dto)
        {
            var response = await _customerService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = PolicyNames.OrdersRead)]
        public async Task<ActionResult<CustomerResponseDto>> GetById(Guid id)
        {
            var response = await _customerService.GetByIdAsync(id);
            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [HttpGet]
        [Authorize(Policy = PolicyNames.OrdersRead)]
        public async Task<ActionResult<PagedResponseDto<CustomerResponseDto>>> GetPaged(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            return Ok(await _customerService.GetPagedAsync(search, page, pageSize));
        }
    }
}