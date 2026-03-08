namespace SADC_Order_Management_System.Models
{
    public class ResponseModel<T>
    {
        public int Total { get; set; }
        public string Status { get; set; } = "success";
        public string Message { get; set; } = string.Empty;
        public string? Errors { get; set; }
        public List<T> Data { get; set; } = new();
    }
}