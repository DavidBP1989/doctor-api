using doctor.Models.Doctor;
using doctor.Models.Doctor.Req;
using doctor.Models.Patients.Req;
using Postal;

namespace doctor.Models
{
    public class EmailTemplate : Email
    {
        public string To { get; set; }
        public string Bcc { get; set; }
        public string Subject { get; set; }

        /*
         * --> propiedades necesarias para enviar
         * diferentes tipos de correos
         */
        public enum TypeEmail
        {
            forgotPwd,
            doctorRegister,
            patientRegister,
            test
        }

        public string Title { get; set; }
        public TypeEmail TypeEmailToSend { get; set; }
        public ForgotPassword ForgotPassword { get; set; }
        public RegisterReq DoctorRegister { get; set; }
        public NewPatientReq PatientReq { get; set; }
    }
}