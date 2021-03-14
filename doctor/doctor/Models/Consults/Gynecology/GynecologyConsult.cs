using System;

namespace doctor.Models.Consults.Gynecology
{
    public class GynecologyConsult : Consult
    {
        public int MenarcaAge { get; set; }
        public DateTime? LastMenstruationDate { get; set; }
        public int Gestas { get; set; }
        public int Paragestas { get; set; }
        public int Cesareans { get; set; }
        public int Abortions { get; set; }
        public int NewlyBorn { get; set; }
        public int Stillbirth { get; set; }
        public int AgeOfOnsetOfActiveSexualLife { get; set; }
        public string Menacma { get; set; }
        public Options Checkbox { get; set; }
        public string SpecifyOthers { get; set; }
        public Partner Partner { get; set; }
    }

    public class Options
    {
        public bool Oligomenorrea { get; set; }
        public bool Proiomenorrea { get; set; }
        public bool Hipermenorrea { get; set; }
        public bool Dismenorrea { get; set; }
        public bool Dispareunia { get; set; }
        public bool Leucorrea { get; set; }
        public bool Lactorrea { get; set; }
        public bool Amenorrea { get; set; }
        public bool Metrorragia { get; set; }
        public bool Others { get; set; }
    }

    public class Partner
    {
        public bool HasAPartner { get; set; }
        public string Name { get; set; }
        public string Sex { get; set; }
        public string MaritalStatus { get; set; }
        public string GroupRH { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Age { get; set; }
        public string Occupation { get; set; }
        public string Phone { get; set; }
    }
}