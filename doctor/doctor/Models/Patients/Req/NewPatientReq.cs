namespace doctor.Models.Patients.Req
{
    public class NewPatientReq
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string MothersName { get; set; }
        public string FathersName { get; set; }
        public string Phone { get; set; }
        public string Sex { get; set; }
        public string Emails { get; set; }
        public string BirthDate { get; set; }
        public string Allergy { get; set; }
        public string Password { get; set; }
    }
}