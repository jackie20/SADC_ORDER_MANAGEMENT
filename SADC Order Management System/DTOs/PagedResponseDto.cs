namespace SADC_Order_Management_System.DTOs.Responses
{
    public class PagedResponseDto<T>
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<T> Data { get; set; } = new();
    }
}