using System.Collections.Generic;

namespace doctor.Models.Consults
{
    public abstract class Consult
    {
        public BasicConsult BasicConsult { get; set; }
        public PatientConsult PatientConsult { get; set; }
    }

    public class Format
    {
        public string Name { get; set; }
        public List<string> Studies { get; set; }
    }
}