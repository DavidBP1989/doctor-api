using Dapper;
using doctor.Database;
using doctor.Models;
using doctor.Models.Consults.General;
using doctor.Models.Consults.General.Req;
using doctor.Models.Consults.General.Res;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using static doctor.Models.Consults.General.GeneralConsult;

namespace doctor.Services
{
    public class DiagnosticService
    {
        private readonly string connection = "";

        public DiagnosticService()
        {
            connection = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;
        }

        public IEnumerable<Diagnostics> GetListOfDiagnostics(int doctorId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();
                return db.Query<CatdiagnosticoRepository>(@"
                        select * from catdiagnostico
                        where idmedico = @doctorId
                        order by nombre",
                        new
                        {
                            doctorId
                        }).ToList()
                    .Select(x => new Diagnostics
                    {
                        Id = x.idcatdiagnostico,
                        GroupName = x.nombre,
                        List = x.lineas.Split('|').ToList()
                    });
            }
        }

        public DiagnosticRes SaveDiagnostic(int doctorId, DiagnosticReq req)
        {
            var result = new DiagnosticRes();

            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();
                try
                {
                    int affectedRow = db.QuerySingle<int>(@"insert into catdiagnostico
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

        public BasicResponse DeleteDiagnosticById(int diagnosticId)
        {
            var result = new BasicResponse();

            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();
                try
                {
                    int affectedRow = db.Execute(@"
                        delete from catdiagnostico where idcatdiagnostico = @diagnosticId", new
                    {
                        diagnosticId
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

        public List<Format> GetDiagnosticsByConsult(int consultId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                var lines = db.Query<string>(@"
                select Lineas from Diagnosticos
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