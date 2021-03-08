using doctor.Models;
using doctor.Models.Doctor;
using doctor.Models.Doctor.Req;
using doctor.Services;
using System.Web.Http;

namespace doctor.Controllers
{
    [RoutePrefix("api/doctor/{doctorId:Int}")]
    [Authorize(Roles = "User")]
    public class DoctorController : ApiController
    {
        private readonly DoctorService service = new DoctorService();

        [Route("")]
        [HttpGet]
        public BasicInfo GetBasicInformation(int doctorId) =>
            service.GetBasicInformation(doctorId);


        [Route("changePwd")]
        [HttpPut]
        public BasicResponse ChangePassword(int doctorId, [FromBody] ChangePasswordReq req) =>
            service.ChangePassword(doctorId, req);


        [Route("")]
        [HttpPut]
        public BasicResponse UpdateRegister(int doctorId, [FromBody] RegisterReq req) =>
            service.UpdateRegister(doctorId, req);


        [Route("registerInfo")]
        [HttpGet]
        public RegisterInformation GetRegisterInformation(int doctorId) =>
            service.GetRegisterInformation(doctorId);
    }
}
