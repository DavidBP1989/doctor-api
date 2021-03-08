namespace doctor.Models.Consults
{
    public class BasicConsult
    {
        public float? Weight { get; set; }
        public float? Size { get; set; }
        public float? Mass { get; set; }
        public float? Temperature { get; set; }
        public int? BloodPressure_A { get; set; }
        public int? BloodPressure_B { get; set; }
        public string ReasonForConsultation { get; set; }
    }
}