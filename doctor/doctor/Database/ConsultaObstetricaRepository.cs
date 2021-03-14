using System;

namespace doctor.Database
{
    public class ConsultaObstetricaRepository
    {
        public int IdConsultaObstetrica { get; set; }
        public byte? noembarazo { get; set; }
        public byte? abortos { get; set; }
        public DateTime? FechaUltmoParto { get; set; }
        public DateTime? PrimerDiaUltimaMestruacuion { get; set; }
        public byte? ToxemiasPrevias { get; set; }
        public byte? Gestas { get; set; }
        public byte? PartosEutocicos { get; set; }
        public byte? PartosDistocios { get; set; }
        public byte? CesareasPrevia { get; set; }
        public byte? UsoDeForceps { get; set; }
        public byte? Motinatos { get; set; }
        public byte? RMVivos { get; set; }
        public byte? EmbarazoEtopicos { get; set; }
        public byte? EmbrazosComplicadosPrevios { get; set; }
        public string EmbrazoEtopicoExplique { get; set; }
        public string EmbarazosComplicadosExplique { get; set; }
        public byte? NoComplicacionesPertinales { get; set; }
        public string ComplicacionesPerinatalesExplique { get; set; }
        public byte? NoEmbrazosAnormales { get; set; }
        public string EmbarazosAnormalesExplique { get; set; }
        public byte? FU { get; set; }
        public byte? FCF { get; set; }
        public byte? CC { get; set; }
        public byte? CA { get; set; }
        public byte? LF { get; set; }
        public byte? DSP { get; set; }
        public string Posicion { get; set; }
        public string Presentacion { get; set; }
        public string siuacuion { get; set; }
        public string Actitud { get; set; }
        public string MovimientosFetales { get; set; }
        public byte? PesoAproxProducto { get; set; }
        public byte? TA { get; set; }
        public byte? FCM { get; set; }
        public string Edema { get; set; }
        public bool? SeHizoUf { get; set; }
        public int? idconsulta { get; set; }
        public bool? activaSexualmente { get; set; }
        public string EspecifiqueToxemias { get; set; }
        public byte? MotivoDistocia { get; set; }
        public string EspecifiqueMotivoDistocia { get; set; }
        public string EspecifiqueTipoDistocia { get; set; }
        public byte? Partos { get; set; }
        public byte? TipoDistocia { get; set; }
        public string ultrasonido { get; set; }
        public string exploracionFisica { get; set; }
        public string Observaciones { get; set; }
    }
}