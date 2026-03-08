using Microsoft.AspNetCore.Mvc;

namespace SADC_Order_Management_System.Helpers
{
    public static class ProblemDetailsHelper
    {
        public static ProblemDetails Create(int status, string title, string detail)
        {
            return new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Type = $"https://httpstatuses.com/{status}"
            };
        }
    }
}