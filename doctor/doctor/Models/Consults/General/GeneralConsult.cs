using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace doctor.Models.Consults.General
{
    public class GeneralConsult : Consult
    {
        [JsonIgnore]
        public DateTime? ConsultationDate { get; set; }
        public float? HeadCircuference { get; set; }
        public int? HeartRate { get; set; }
        public int? BreathingFrecuency { get; set; }
        public string ReasonForConsultation { get; set; }
        public string PhysicalExploration { get; set; }
        public string PreventiveMeasures { get; set; }
        public string Observations { get; set; }
        public List<Format> Diagnostics { get; set; }
        public List<Format> Treatments { get; set; }
        public List<Format> CabinetStudies { get; set; }
        public List<Format> LaboratoryStudies { get; set; }

        [JsonIgnore]
        public string _Prognostic { get; set; }
        public List<string> Prognostic { get; set; }

        public class Format
        {
            public string Name { get; set; }
            public List<string> Studies { get; set; }
        }
    }
}