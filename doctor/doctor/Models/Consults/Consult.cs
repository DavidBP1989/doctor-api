namespace doctor.Models.Consults
{
    public abstract class Consult
    {
        public BasicConsult BasicConsult { get; set; }
        public PatientConsult PatientConsult { get; set; }
    }
}