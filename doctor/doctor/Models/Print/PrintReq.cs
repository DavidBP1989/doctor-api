namespace doctor.Models.Print
{
    public class PrintReq
    {
        public string TextColor { get; set; }
        public string BgPrimaryColor { get; set; }
        public string BgSecondaryColor { get; set; }
        public ImageReq ImageLogo { get; set; }
    }
}