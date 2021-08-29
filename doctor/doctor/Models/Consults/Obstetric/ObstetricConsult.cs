using System;
using System.Collections.Generic;

namespace doctor.Models.Consults.Obstetric
{
    public class ObstetricConsult : Consult
    {
        public byte PregnancyNumber { get; set; }
        public bool SexuallyActive { get; set; }
        public byte Abortions { get; set; }
        public DateTime? LastParturitionDate { get; set; }
        public DateTime? FirstDayOfLastMenstruation { get; set; }
        public bool PreviousToxemias { get; set; }
        public string SpecifyToxemias { get; set; }
        public byte Parturition { get; set; }
        public int DystociaType { get; set; }
        public string SpecifyDystociaType { get; set; }
        public int DystociaReason { get; set; }
        public string SpecifyDystociaReason { get; set; }
        public byte PreviousCesarean { get; set; }
        public byte Forceps { get; set; }
        public byte Stillbirths { get; set; }
        public byte NewbornAlive { get; set; }
        public bool EctopicPregnancies { get; set; }
        public string SpecifyEctopicPregnancies { get; set; }
        public bool PreviousPregnacyComplications { get; set; }
        public string SpecifyPreviousPregnacyComplications { get; set; }
        public bool PerinatalComplications { get; set; }
        public string SpecifyPerinatalComplications { get; set; }
        public bool AbnormalPregnancies { get; set; }
        public string SpecifyAbnormalPregnancies { get; set; }
        public string Observations { get; set; }
        public PregnancyControl PregnancyControl { get; set; }
        public List<Format> Diagnostics { get; set; }
        public List<Format> Treatments { get; set; }
        public List<Format> CabinetStudies { get; set; }
        public List<Format> LaboratoryStudies { get; set; }
    }

    public class PregnancyControl
    {
        public byte FU { get; set; }
        public byte FCF { get; set; }
        public byte CC { get; set; }
        public byte CA { get; set; }
        public byte LF { get; set; }
        public byte DBP { get; set; }
        public string Position { get; set; }
        public string Presentation { get; set; }
        public string Situtation { get; set; }
        public string Attitude { get; set; }
        public string FetalMovements { get; set; }
        public double ApproximateProductWeight { get; set; }
        public byte TA { get; set; }
        public byte FCM { get; set; }
        public string Edema { get; set; }
        public bool MadeUf { get; set; }
        public string Ultrasound { get; set; }
        public string PhysicalExploration { get; set; }
    }
}