namespace doctor.Models.Doctor.Req
{
    public class RegisterReq
    {
        //informacion del especialista
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        public string RFC { get; set; }
        public string CURP { get; set; }
        public string NoSEP_ProfessionalCertificate { get; set; }
        public string NoSSA { get; set; }
        public string NoCertification_CMCP { get; set; }
        public string ProfessionalResidenceHospital { get; set; }
        public string UniversitySpecialty { get; set; }
        public string SpecialtyCertificate { get; set; }
        public string NameStateSchool { get; set; }
        public string NameStateGrouping { get; set; }
        public int? MedicalSpeciality { get; set; }
        public string SubmedicalSpeciality { get; set; }

        //domicilio
        public string Address { get; set; }
        public string Colony { get; set; }
        public string PostalCode { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string OfficePhone { get; set; }
        public string OfficeAddress { get; set; }

        //contacto
        public string Phone { get; set; }
        public string CellPhone { get; set; }
        public string Email { get; set; }

        //estados
        public string JsonStateList { get; set; }
    }
}