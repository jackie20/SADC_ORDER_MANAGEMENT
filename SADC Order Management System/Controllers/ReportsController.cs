using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SADC_Order_Management_System.Authorization;
using SADC_Order_Management_System.DTOs.Responses;
using SADC_Order_Management_System.Services.Interfaces;

namespace SADC_Order_Management_System.Controllers
{
    [ApiController]
    [Route("api/reports/orders")]
    public class ReportsController : ControllerBase
    {
        private readonly IFxService _fxService;

        public ReportsController(IFxService fxService)
        {
            _fxService = fxService;
        }

        [HttpGet("zar")]
        [Authorize(Policy = PolicyNames.OrdersRead)]
        public async Task<ActionResult<OrdersZarReportResponseDto>> GetOrdersInZar()
        {
            return Ok(await _fxService.GetOrdersZarReportAsync());
        }
    }
}