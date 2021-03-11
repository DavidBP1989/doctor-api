using System;

namespace doctor.Database
{
    public class ConsultasRepository
    {
        public int? idpaciente { get; set; }
        public int idconsulta { get; set; }
        public DateTime? Fecha { get; set; }
        public float? Peso { get; set; }
        public float? Altura { get; set; }
        public float? Temperatura { get; set; }
        public float? Cabeza { get; set; }
        public float? perimetroCefalico { get; set; }
        public int? TensionArterial { get; set; }
        public int? TensionArterialB { get; set; }
        public int? FrecuenciaCardiaca { get; set; }
        public int? FrecuenciaRespiratoria { get; set; }
        public int? idmedico { get; set; }
        public string motivo { get; set; }
        public string SignosSintomas1 { get; set; }
        public string SignosSintomas2 { get; set; }
        public string SignosSintomas3 { get; set; }
        public string MedidasPreventivas { get; set; }
        public DateTime? ProximaCita { get; set; }
        public string observaciones { get; set; }
        public string Pronostico { get; set; }
    }
}