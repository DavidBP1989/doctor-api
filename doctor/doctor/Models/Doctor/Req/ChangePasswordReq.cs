namespace doctor.Models.Doctor.Req
{
    public class ChangePasswordReq
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; } = "";
    }
}