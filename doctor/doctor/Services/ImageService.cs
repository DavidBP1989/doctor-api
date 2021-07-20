using doctor.Models.Print;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Web;
using static System.IO.Directory;

namespace doctor.Services
{
    public class ImageService
    {
        public ImageRes AddImage(int doctorId, ImageReq req)
        {
            var result = new ImageRes();
            string root = AppDomain.CurrentDomain.BaseDirectory;
            string folder = $"{root}Content\\ImageLogo\\{doctorId}";

            if (!Exists(folder)) _ = CreateDirectory(folder);

            string file = $"{folder}\\{req.Title}";
            if (Exists(file))
                File.Delete(file);

            var img = new ImageService().Base64ToImage(req.Base64Image);
            if (img != null)
            {
                try
                {
                    img.Save(file);
                    result.UrlImage = GetUrlImage(doctorId, req.Title);
                    result.TitleWithExtension = req.Title;
                }
                catch (Exception ex)
                {
                    Log.Write($"Error al guardar el logo: {ex.Message}");
                }
            }
            return result;
        }

        private Image Base64ToImage(string base64String)
        {
            Image img = null;
            try
            {
                byte[] buffer = Convert.FromBase64String(base64String);
                img = Image.FromStream(new MemoryStream(buffer, 0, buffer.Length), true);
            }
            catch { }

            return img;
        }

        public string GetDefaultImage()
        {
            return GetUrlImage(0, null, true);
        }

        public string GetUrlImage(int doctorId, string title, bool defaultImage = false)
        {
            string appname = ConfigurationManager.AppSettings["appname"] ?? "";
            if (!string.IsNullOrEmpty(appname)) appname = $"/{appname}";

            var url = $"{HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority)}{appname}/Content/ImageLogo/";
            return url + (defaultImage ? "defaultLogo.png" : $"{doctorId}/{title}");
        }
    }
}