using Dapper;
using doctor.Database;
using doctor.Models.Patients;
using doctor.Models.Patients.Req;
using doctor.Models.Patients.Res;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using drawing = System.Drawing;

namespace doctor.Services
{
    public class PatientService
    {
        private readonly string connection = "";

        public PatientService()
        {
            connection = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;
        }

        public async Task<Tuple<int, IEnumerable<Patients>>> GetListOfPatients(int doctorId, int? page, int? itemsPerPage, string columnName, string textToSearch, string orderby)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();
                var emeci = GetEmeci(doctorId);
                int _page = (int)((page - 1) * itemsPerPage);

                var result = await db.QueryMultipleAsync("spPatientList", new
                {
                    emeci,
                    columnName,
                    @textToSearch = textToSearch.Replace(' ', '%'),
                    @page = _page,
                    itemsPerPage,
                    orderby
                }, null, null, CommandType.StoredProcedure);

                int totalRows = result.Read<int>().First();

                var _result = result.Read<Patients>().ToList();
                foreach (var r in _result) r.AgeInMonths = AgeInMonths(r.BirthDate);

                return new Tuple<int, IEnumerable<Patients>>(totalRows, _result);
            }
        }

        public string GetLastEmeci(int doctorId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                string emeci = GetEmeci(doctorId);
                var lastEmeciFromPatient = db.Query<string>(@"
                    select top(1) Emeci
                    from vPatients where Emeci like @emeci
                    order by RegistrationDate desc",
                    new
                    {
                        emeci = "%" + emeci + "%"
                    }).FirstOrDefault();
                if (lastEmeciFromPatient != null)
                {
                    string[] split = lastEmeciFromPatient.Split('-');
                    int last = int.Parse(split[2]);
                    last++;
                    return $"{split[0]}-{split[1]}-{last:000#}";
                }
            }
            return "";
        }

        public Patient GetByPatientById(int patientId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                var patient = db.Query<Patient>(@"select * from vPatients where Id = @patientId",
                    new { patientId }).FirstOrDefault();

                if (patient != null)
                {
                    patient.AgeInMonths = AgeInMonths(patient.BirthDate);
                    var alergenos = db.Query<string>(@"
                            select Alergeno from Patologias
                            where idpaciente = @patientId and Categoria = 5",
                            new { patientId }).ToList();
                    if (alergenos.Count > 0 && string.IsNullOrEmpty(patient.Allergies))
                    {
                        patient.Allergies = "";
                        foreach (var a in alergenos)
                        {
                            patient.Allergies += a + "\r\n";
                        }
                    }

                    var coordinates = db.Query<DatosTarjetaRepository>(@"
                        select * from DatosTarjeta where noTarjeta = @emeci",
                        new { emeci = patient.EMECI }).ToList();
                    var random = coordinates.ElementAt(new Random().Next(1, coordinates.Count()));
                    patient.RandomCoordinate = random.Coordenada;
                    patient.RandomCoordinateValue = random.Dato;
                }
                return patient;
            }
        }


        #region newPatient
        public NewPatientRes AddNewPatient(int doctorId, NewPatientReq req)
        {
            var result = new NewPatientRes();
            string emeci = GetLastEmeci(doctorId);
            MemoryStream draw = null;

            using (IDbConnection db = new SqlConnection(connection))
            {
                var cn = db;
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    var trans = transaction;

                    try
                    {
                        int affectedRowRegister = InsertRegister(ref cn, ref trans, emeci, req);
                        int affectedRowPatient = 0;
                        if (affectedRowRegister > 0)
                            affectedRowPatient = InsertPatient(ref cn, ref trans, affectedRowRegister, req);

                        if (affectedRowRegister > 0 && affectedRowPatient > 0)
                        {
                            draw = DrawDataInCard(ref cn, ref trans, emeci);

                            transaction.Commit();
                            result.PatientId = affectedRowPatient;
                            result.IsSuccess = true;
                        }
                    }
                    catch (DBConcurrencyException ex)
                    {
                        result.Error = ex.Message;
                        Log.Write(ex.Message);
                    }

                    if (result.PatientId.HasValue && draw != null)
                    {
                        var emailService = new EmailService(req.Emails);
                        Task.Run(async () =>
                        {
                            await emailService.SendPatientRegister(req, draw);
                        });
                    }
                }
            }
            return result;
        }

        private int InsertRegister(ref IDbConnection db, ref IDbTransaction transaction, string emeci, NewPatientReq req)
        {
            return db.QuerySingle<int>(@"
                    insert into Registro
                    (Nombre, Apellido, Telefono, Tipo, Status, FechaRegistro,
                    FechaExpiracion, Emails, clave, Emeci)
                    values
                    (@name, @lastName, @phone, @type, @status, @registrationDate,
                    @expirationDate, @email, @password, @emeci);
                    select cast(scope_identity() as int)",
                    new
                    {
                        name = req.Name,
                        lastName = req.LastName,
                        phone = req.Phone,
                        type = "P",
                        status = "V",
                        registrationDate = DateTime.Now.Date,
                        expirationDate = DateTime.Now.AddMonths(1).Date,
                        email = req.Emails,
                        password = req.Password,
                        emeci,
                    }, transaction);
        }

        private int InsertPatient(ref IDbConnection db, ref IDbTransaction transaction, int registerId, NewPatientReq req)
        {
            return db.QuerySingle<int>(@"
                    insert into Paciente
                    (IdRegistro, Sexo, FechaNacimiento, NombreMadre, NombrePadre, AlergiaMedicina)
                    values
                    (@registerId, @sex, @birthDate, @mothersName, @fathersName, @allergies);
                    select cast(scope_identity() as int)",
                    new
                    {
                        registerId,
                        sex = req.Sex,
                        birthDate = DateTime.Parse(req.BirthDate),
                        mothersName = req.MothersName,
                        fathersName = req.FathersName,
                        allergies = req.Allergy
                    }, transaction);
        }

        private MemoryStream DrawDataInCard(ref IDbConnection db, ref IDbTransaction transaction, string emeci)
        {
            var memory = new MemoryStream();
            var doc = new Document();
            var pdf = PdfWriter.GetInstance(doc, memory);

            doc.Open();

            var bfTimes = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);
            var fBold = new Font(bfTimes, 8, Font.NORMAL, BaseColor.BLACK);

            var pdfTable = new PdfPTable(2)
            {
                TotalWidth = 500f,
                LockedWidth = true,
                HorizontalAlignment = 1
            };

            var widths = new float[] { 2.8f, 2.1f };
            pdfTable.SetWidths(widths);
            pdfTable.DefaultCell.Border = Rectangle.NO_BORDER;

            var file = $"{AppDomain.CurrentDomain.BaseDirectory}Content\\imgAccess.jpg";
            var image = Image.GetInstance(ConvertImageToBytes(file, emeci));
            image.ScaleAbsolute(258f, 153f);

            var cell = new PdfPCell(image)
            {
                HorizontalAlignment = Element.ALIGN_MIDDLE,
                Border = Rectangle.NO_BORDER
            };
            pdfTable.AddCell(cell);

            var position = new PdfPTable(11)
            {
                HorizontalAlignment = 1,
                TotalWidth = 265f,
                LockedWidth = true
            };

            var cellPosition = new PdfPCell(new Phrase("Posiciones de Acceso Seguro"))
            {
                BackgroundColor = new BaseColor(162, 212, 255),
                HorizontalAlignment = 1,
                Colspan = 11,
                Border = Rectangle.NO_BORDER
            };
            position.AddCell(cellPosition);

            string[] arrABC = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            for (var i = 0; i <= 10; i++)
            {
                position.DefaultCell.BackgroundColor = new BaseColor(255, 255, 255);
                position.DefaultCell.Padding = 2f;
                position.DefaultCell.Border = Rectangle.NO_BORDER;

                if (i == 0)
                    position.AddCell("");
                else position.AddCell(new Phrase(arrABC[i - 1], fBold));
            }

            bool color = true;

            for (int i = 1; i <= 10; i++)
            {
                if (color)
                    position.DefaultCell.BackgroundColor = new BaseColor(162, 212, 255);
                else position.DefaultCell.BackgroundColor = new BaseColor(255, 255, 255);

                color = !color;

                position.AddCell(new Phrase(i.ToString(), fBold));

                string letter;
                for (var j = 0; j <= 9; j++)
                {
                    letter = arrABC[j];
                    var dato = new Random().Next(0, 999).ToString("00#");
                    db.Execute(@"
                            insert into DatosTarjeta
                            (noTarjeta, Dato, Coordenada)
                            values (@emeci, @dato, @coordinate)",
                            new
                            {
                                emeci,
                                dato,
                                coordinate = $"{letter}{i}"
                            }, transaction);

                    position.AddCell(new Phrase(dato, fBold));
                }
            }

            pdfTable.AddCell(position);
            doc.Add(pdfTable);

            pdf.CloseStream = false;
            doc.Close();
            memory.Position = 0;

            return memory;
        }
        #endregion


        public NewPatientRes FindExistingPatient(NewExistingPatientReq req)
        {
            var result = new NewPatientRes();
            using (IDbConnection db = new SqlConnection(connection))
            {
                var patient = db.Query<int>(@"select Id from vPatients where Emeci = @emeci",
                    new { emeci = req.Emeci }).FirstOrDefault();

                if (patient > 0)
                {
                    var dt = db.Query<bool>(@"
                        select * from DatosTarjeta
                        where Coordenada = @coordinate and
                        Dato = @dato and
                        noTarjeta = @emeci", new
                    {
                        coordinate = req.Coordinate,
                        dato = req.Value,
                        emeci = req.Emeci
                    }).Any();

                    if (dt)
                    {
                        result.PatientId = patient;
                        result.IsSuccess = true;
                    }
                }
            }
            return result;
        }

        private string GetEmeci(int doctorId)
        {
            return new DoctorService().GetBasicInformation(doctorId)?.EMECI;
        }

        private int AgeInMonths(DateTime? birthDate)
        {
            if (!birthDate.HasValue)
                return -1;
            return ((DateTime.Now.Year - birthDate.Value.Year) * 12) + (DateTime.Now.Month - birthDate.Value.Month);
        }

        private byte[] ConvertImageToBytes(string file, string msj)
        {
            var bitImg = new drawing.Bitmap(file);
            var gImg = drawing.Graphics.FromImage(bitImg);

            gImg.SmoothingMode = drawing.Drawing2D.SmoothingMode.AntiAlias;
            gImg.DrawString(
                msj,
                new drawing.Font("Arial", 20, drawing.FontStyle.Bold),
                drawing.SystemBrushes.Window, new drawing.Point(50, 175)
            );

            var ms = new MemoryStream();
            bitImg.Save(ms, drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }
    }
}