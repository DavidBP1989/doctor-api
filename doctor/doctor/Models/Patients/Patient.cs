namespace doctor.Models.Patients
{
    public class Patient : Patients
    {
        public string GroupRH { get; set; }
        public string Allergies { get; set; }
        public string Reserved { get; set; }
        public string RelevantPathologies { get; set; }
        public string RandomCoordinate { get; set; }
        public string RandomCoordinateValue { get; set; }
    }
}