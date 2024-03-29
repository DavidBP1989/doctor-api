﻿using doctor.Models;
using doctor.Models.Doctor;
using doctor.Models.Doctor.Req;
using doctor.Models.Patients.Req;
using doctor.Models.Patients.Res;
using doctor.Services;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Web.Http;

namespace doctor.Controllers
{
    [RoutePrefix("api/wAuthority")]
    public class PublicController : ApiController
    {
        private readonly DoctorService doctorService = new DoctorService();
        private readonly GeneralService generalService = new GeneralService();
        private readonly PatientService patientService = new PatientService();

        [Route("doctorregister")]
        [HttpPost]
        public BasicResponse DoctorRegister([FromBody] RegisterReq req)
        {
            var result = doctorService.Register(req);
            if ((bool)(result?.IsSuccess))
            {
                var emailService = new EmailService(req.Email);
                //Task.Run(async () =>
                //{
                //    await emailService.SendDoctorRegister(req);
                //});
                emailService.SendDoctorRegister(req);
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
                //Task.Run(async () =>
                //{
                //    await emailService.SendForgotPassword(fp);
                //});
                emailService.SendForgotPassword(fp);
            }
            return result;
        }


        [HttpGet]
        [Route("doctorsByAssociation/{associationId}")]
        public IEnumerable<DoctorsList> GetListOfDoctorByAssociation(string associationId) =>
            doctorService.GetListOfDoctorByAssociation(associationId);


        [HttpPost]
        [Route("patientregister/{doctorId:Int}")]
        public NewPatientRes PatientRegister(int doctorId, [FromBody] NewPatientReq req) =>
            patientService.AddNewPatient(doctorId, req);


        [HttpGet]
        [Route("medicalSpecialties")]
        public IEnumerable<MedicalSpecialties> GetMedicalSpecialties() => generalService.GetMedicalSpecialties();

        [HttpGet]
        [Route("emailtest")]
        public IHttpActionResult EmailTest()
        {
            string message = "";
            var email = new EmailTemplate
            {
                To = "bustamante24.1989@gmail.com, info@emeci.com",
                Bcc = "leo26102019@gmail.com",
                Subject = "EMECI - prueba de correo",
                Title = "PRUEBA",
                TypeEmailToSend = EmailTemplate.TypeEmail.test
            };

            try
            {
                email.Send();
            }
            catch (Exception ex) { message = ex.Message; }

            return Ok(new
            {
                message
            });
        }
    }
}
