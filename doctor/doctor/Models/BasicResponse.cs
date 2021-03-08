namespace doctor.Models
{
    public class BasicResponse
    {
        public bool IsSuccess { get; set; } = false;
        public string Error { get; set; }
    }
}