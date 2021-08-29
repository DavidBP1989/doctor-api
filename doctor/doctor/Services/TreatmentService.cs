using Dapper;
using doctor.Database;
using doctor.Models;
using doctor.Models.Consults;
using doctor.Models.Consults.General;
using doctor.Models.Consults.General.Req;
using doctor.Models.Consults.General.Res;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace doctor.Services
{
    public class TreatmentService
    {
        private readonly string connection = "";
        public TreatmentService()
        {
            connection = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;
        }

        public IEnumerable<Treatments> GetListOfTreatments(int doctorId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();
                return db.Query<CatrecetasRepository>(@"
                        select * from catrecetas
                        where idmedico = @doctorId
                        order by nombre",
                        new
                        {
                            doctorId
                        }).ToList()
                    .Select(x => new Treatments
                    {
                        Id = x.idcatreceta,
                        GroupName = x.nombre,
                        List = x.lineas.Split('|').ToList()
                    });
            }
        }

        public TreatmentRes SaveTreatment(int doctorId, TreatmentReq req)
        {
            var result = new TreatmentRes();

            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();
                try
                {
                    int affectedRow = db.QuerySingle<int>(@"insert into catrecetas
                    (idmedico, nombre, lineas) values (@doctorId, @name, @lines);
                    select cast(scope_identity() as int)",
                    new
                    {
                        doctorId,
                        name = req.GroupName,
                        lines = string.Join("|", req.List)
                    });

                    if (affectedRow > 0)
                    {
                        result.IsSuccess = true;
                        result.Id = affectedRow;
                    }
                }
                catch (DBConcurrencyException ex)
                {
                    result.Error = ex.Message;
                    Log.Write(ex.Message);
                }
            }
            return result;
        }

        public BasicResponse DeleteTreatmentById(int treatmentId)
        {
            var result = new BasicResponse();

            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();
                try
                {
                    int affectedRow = db.Execute(@"
                        delete from catrecetas where idcatreceta = @treatmentId", new
                    {
                        treatmentId
                    });

                    result.IsSuccess = affectedRow > 0;
                }
                catch (DBConcurrencyException ex)
                {
                    result.Error = ex.Message;
                    Log.Write(ex.Message);
                }
            }
            return result;
        }

        public List<Format> GetTreatmentsByConsult(int consultId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                var lines = db.Query<string>(@"
                select Lineas from Recetas
                where idconsulta = @consultId", new
                {
                    consultId
                }).FirstOrDefault();

                var newFormat = lines?.StartsWith("[new]");

                return (newFormat.HasValue && newFormat.Value) ? Helper.NewLineformat(lines) : Helper.LineFormat(lines);
            }
        }
    }
}