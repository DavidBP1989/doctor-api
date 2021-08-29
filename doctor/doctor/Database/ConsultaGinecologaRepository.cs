using System;

namespace doctor.Database
{
    public class ConsultaGinecologaRepository
    {
        public int idconsultaginecologa { get; set; }
        public int idconsulta { get; set; }
        public DateTime? FechaUltimaMestruacion { get; set; }
        public byte? Gestas { get; set; }
        public byte? ParaGestas { get; set; }
        public byte? Cesareas { get; set; }
        public byte? abortos { get; set; }
        public byte? RecienNacidosVivos { get; set; }
        public byte? mortinatos { get; set; }
        public byte? EdadInicioVidaSexual { get; set; }
        public string menacma { get; set; }
        public bool? oligonorrea { get; set; }
        public bool? Proiomenorrea { get; set; }
        public bool? Hipermenorrea { get; set; }
        public bool? Dismenorrea { get; set; }
        public bool? Dispareunia { get; set; }
        public bool? Leucorrea { get; set; }
        public bool? Lactorrea { get; set; }
        public bool? Amenorrea { get; set; }
        public bool? Metrorragia { get; set; }
        public bool? Otros { get; set; }
        public string OtrosEspecifique { get; set; }
        public bool? TienePareja { get; set; }
        public string SexoPareja { get; set; }
        public string EstadoCivilPareja { get; set; }
        public string GrupoRHPareja { get; set; }
        public DateTime? FechaNacimientoPareja { get; set; }
        public string OcupacionPareja { get; set; }
        public string TelefonoPareja { get; set; }
        public string nombrePareja { get; set; }
        public string edadPareja { get; set; }
        public bool? SexuallyActive { get; set; }
    }
}