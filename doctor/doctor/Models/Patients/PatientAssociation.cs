using System;

namespace doctor.Models.Patients
{
    public class PatientAssociation
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Sexo { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public int EdadEnMeses { get; set; }
    }
}