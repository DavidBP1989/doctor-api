using Dapper;
using doctor.Models;
using doctor.Models.Print;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace doctor.Services
{
    public class PrintService
    {
        private readonly string connection = "";

        public PrintService()
        {
            connection = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;
        }

        #region newConfig
        public BasicResponse AddNewConfig(int doctorId, PrintReq req)
        {
            var result = new BasicResponse();
            using (IDbConnection db = new SqlConnection(connection))
            {
                var cn = db;
                db.Open();

                var updateRegister = db.ExecuteScalar<bool>(@"
                    select count(1) from PrintConfig
                    where DoctorId = @doctorId",
                    new { doctorId });

                try
                {
                    if (updateRegister)
                    {
                        Update(ref cn, doctorId, req);
                        result.IsSuccess = true;
                    } else result.IsSuccess = Insert(ref cn, doctorId, req) > 0;
                }
                catch (DBConcurrencyException ex)
                {
                    result.Error = ex.Message;
                    Log.Write(ex.Message);
                }
            }
            return result;
        }

        private void Update(ref IDbConnection db, int doctorId, PrintReq req)
        {
            var image = GetImage(doctorId, req);

            if (image != null)
            {
                db.Execute(@"update PrintConfig
                    set ImageRef = @image,
                    TextColor = @textColor,
                    BgColor1 = @bg1,
                    BgColor2 = @bg2
                    where DoctorId = @doctorId", new
                    {
                        image = image?.TitleWithExtension,
                        textColor = req.TextColor,
                        bg1 = req.BgPrimaryColor,
                        bg2 = req.BgSecondaryColor,
                        doctorId
                    });
            } else
            {
                db.Execute(@"update PrintConfig
                    set TextColor = @textColor,
                    BgColor1 = @bg1,
                    BgColor2 = @bg2
                    where DoctorId = @doctorId", new
                {
                    textColor = req.TextColor,
                    bg1 = req.BgPrimaryColor,
                    bg2 = req.BgSecondaryColor,
                    doctorId
                });
            }
        }

        private int Insert(ref IDbConnection db, int doctorId, PrintReq req)
        {
            var image = GetImage(doctorId, req);

            return db.QuerySingle<int>(@"
                    insert into PrintConfig
                    (DoctorId, ImageRef, TextColor, BgColor1, BgColor2)
                    values
                    (@doctorId, @image, @txtColor, @bg1, @bg2);
                    select cast(scope_identity() as int)",
                    new
                    {
                        doctorId,
                        image = image?.TitleWithExtension,
                        txtColor = req.TextColor,
                        bg1 = req.BgPrimaryColor,
                        bg2 = req.BgSecondaryColor
                    });
        }

        private ImageRes GetImage(int doctorId, PrintReq req)
        {
            ImageRes image = null;
            if (req.ImageLogo != null && req.ImageLogo.Base64Image != null)
                image = new ImageService().AddImage(doctorId, req.ImageLogo);
            return image;
        }
        #endregion


        public PrintRes GetPrintConfig(int doctorId)
        {
            PrintRes result = null;
            var imageService = new ImageService();

            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();
                result = db.Query<PrintRes>(@"
                    select ImageRef,
                    TextColor,
                    BgColor1 as BgPrimaryColor,
                    BgColor2 as BgSecondaryColor
                    from PrintConfig where DoctorId = @doctorId",
                    new
                    {
                        doctorId
                    }).FirstOrDefault();
                if (result == null)
                {
                    result = new PrintRes
                    {
                        IsDefault = true,
                        UrlImage = imageService.GetDefaultImage()
                    };
                }
                else
                {
                    result.UrlImage = !string.IsNullOrEmpty(result.ImageRef)
                        ? imageService.GetUrlImage(doctorId, result.ImageRef)
                        : imageService.GetDefaultImage();
                    result.IsDefault = false;
                }
            }
            return result;
        }
    }
}