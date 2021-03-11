﻿using doctor.Models;
using doctor.Models.Doctor;
using doctor.Models.Doctor.Req;
using doctor.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace doctor.Controllers
{
    [RoutePrefix("api/wAuthority")]
    public class PublicController : ApiController
    {
        private readonly DoctorService doctorService = new DoctorService();
        private readonly GeneralService generalService = new GeneralService();

        [Route("doctorregister")]
        [HttpPost]
        public BasicResponse Register([FromBody] RegisterReq req)
        {
            var result = doctorService.Register(req);
            if ((bool)(result?.IsSuccess))
            {
                var emailService = new EmailService(req.Email);
                Task.Run(async () =>
                {
                    await emailService.SendDoctorRegister(req);
                });
            }
            return result;
        }


        [Route("states")]
        [HttpGet]
        public IEnumerable<States> GetStates() => generalService.GetStates();


        [Route("cities/{stateId}")]
        [HttpGet]
        public IEnumerable<Cities> GetCities(string stateId) => generalService.GetCities(stateId);


        [HttpGet]
        [Route("forgotpwd/{emeci}")]
        public BasicResponse ForgotPassword(string emeci)
        {
            var fp = generalService.ForgotPassword(emeci);
            var result = new BasicResponse();

            if (fp != null && !string.IsNullOrEmpty(fp.Password) && !string.IsNullOrEmpty(fp.Email))
            {
                result.IsSuccess = true;
                var emailService = new EmailService(fp.Email);
                Task.Run(async () =>
                {
                    await emailService.SendForgotPassword(fp);
                });
            }
            return result;
        }
    }
}
