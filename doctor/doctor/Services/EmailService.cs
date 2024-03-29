﻿using doctor.Models;
using doctor.Models.Doctor;
using doctor.Models.Doctor.Req;
using doctor.Models.Patients.Req;
using System;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;

namespace doctor.Services
{
    public class EmailService
    {
        private const string Bcc = "javier@emeci.com";
        public string To { get; set; }
        public EmailService(string to)
        {
            To = to;
        }

        public async Task SendForgotPasswordAsync(ForgotPassword req)
        {
            var email = new EmailTemplate
            {
                To = To,
                Bcc = Bcc,
                Subject = "EMECI - Recordatorio de contraseña",
                Title = "Protege tu salud y la de tu familia a través de EMECI",
                TypeEmailToSend = EmailTemplate.TypeEmail.forgotPwd,
                ForgotPassword = req
            };

            try
            {
                await email.SendAsync();
            }
            catch (Exception ex)
            {
                Log.Write($"Error al enviar el correo de recuperar contraseña: {ex.Message}");
            }
        }

        public void SendForgotPassword(ForgotPassword req)
        {
            var email = new EmailTemplate
            {
                To = To,
                Bcc = Bcc,
                Subject = "EMECI - Recordatorio de contraseña",
                Title = "Protege tu salud y la de tu familia a través de EMECI",
                TypeEmailToSend = EmailTemplate.TypeEmail.forgotPwd,
                ForgotPassword = req
            };

            try
            {
                email.Send();
            }
            catch (Exception ex)
            {
                Log.Write($"Error al enviar el correo de recuperar contraseña: {ex.Message}");
            }
        }

        public async Task SendDoctorRegisterAsync(RegisterReq req)
        {
            var email = new EmailTemplate
            {
                To = To,
                Bcc = Bcc,
                Subject = "EMECI - Registro médico",
                Title = "Registro médico",
                TypeEmailToSend = EmailTemplate.TypeEmail.doctorRegister,
                DoctorRegister = req
            };

            try
            {
                await email.SendAsync();
            }
            catch (Exception ex)
            {
                Log.Write($"Error al enviar el correo de registro de doctor: {ex.Message}");
            }
        }

        public void SendDoctorRegister(RegisterReq req)
        {
            var email = new EmailTemplate
            {
                To = To,
                Bcc = Bcc,
                Subject = "EMECI - Registro médico",
                Title = "Registro médico",
                TypeEmailToSend = EmailTemplate.TypeEmail.doctorRegister,
                DoctorRegister = req
            };

            try
            {
                email.Send();
            }
            catch (Exception ex)
            {
                Log.Write($"Error al enviar el correo de registro de doctor: {ex.Message}");
            }
        }

        public async Task SendPatientRegisterAsync(NewPatientReq req, MemoryStream positions)
        {
            var email = new EmailTemplate
            {
                To = To,
                Bcc = Bcc,
                Subject = "EMECI - Registro de paciente",
                Title = "Registro de paciente",
                TypeEmailToSend = EmailTemplate.TypeEmail.patientRegister,
                PatientReq = req
            };

            email.Attach(new Attachment(positions, "PosicionesDeAcceso.pdf"));

            try
            {
                await email.SendAsync();
            }
            catch (Exception ex)
            {
                Log.Write($"Error al enviar el correo de nuevo pacient: {ex.Message}");
            }
        }

        public void SendPatientRegister(NewPatientReq req, MemoryStream positions)
        {
            var email = new EmailTemplate
            {
                To = To,
                Bcc = Bcc,
                Subject = "EMECI - Registro de paciente",
                Title = "Registro de paciente",
                TypeEmailToSend = EmailTemplate.TypeEmail.patientRegister,
                PatientReq = req
            };

            email.Attach(new Attachment(positions, "PosicionesDeAcceso.pdf"));

            try
            {
                email.Send();
            }
            catch (Exception ex)
            {
                Log.Write($"Error al enviar el correo de nuevo pacient: {ex.Message}");
            }
        }
    }
}