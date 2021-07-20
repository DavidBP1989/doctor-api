using Newtonsoft.Json;

namespace doctor.Models.Print
{
    public class PrintRes
    {
        public bool IsDefault { get; set; }
        public string TextColor { get; set; }
        public string BgPrimaryColor { get; set; }
        public string BgSecondaryColor { get; set; }
        [JsonIgnore]
        public string ImageRef { get; set; }
        public string UrlImage { get; set; }
    }
}