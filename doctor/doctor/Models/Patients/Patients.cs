using System;

namespace doctor.Models.Patients
{
    public class Patients
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public int AgeInMonths { get; set; }
        public DateTime? LastConsultation { get; set; }
        public string EMECI { get; set; }
        public string Sex { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}