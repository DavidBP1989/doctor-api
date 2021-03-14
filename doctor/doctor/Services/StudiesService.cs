using Dapper;
using doctor.Database;
using doctor.Models.Consults.General;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace doctor.Services
{
    public class StudiesService
    {
        private readonly string connection = "";
        public StudiesService()
        {
            connection = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;
        }

        public enum TypeCategory
        {
            Laboratory = 0,
            Cabinet = 1
        }

        public IEnumerable<CategoryStudies> GetStudiesByType(TypeCategory type)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                var catStudies = db.Query<CatCategoEstudiosRepository>(@"
                        select * from CatCategoEstudios where Tipo = @type", new
                {
                    type = (int)type
                }).ToList();

                var studies = db.Query<CatEstudiosRepository>(@"
                        select * from CatEstudios").ToList();

                return catStudies
                .Select(x => new CategoryStudies
                {
                    Id = x.idcategoriaestudio,
                    Name = x.descripcion,
                    StudiesList = studies
                    .Where(s => s.idcategoriaestudio == x.idcategoriaestudio)
                    .Select(s => new CategoryStudies.Studies
                    {
                        Id = s.idestudio,
                        Name = s.descripcion
                    }).ToList()
                }).ToList();
            }
        }

        public List<string> GetLaboratoryStudies(int consultId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                var lines = db.Query<string>(@"
                select Lineas from EstudiosLab
                where idconsulta = @consultId", new
                {
                    consultId
                }).FirstOrDefault();
                return Helper.LineFormat(lines);
            }
        }

        public List<string> GetCabinetStudies(int consultId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                var lines = db.Query<string>(@"
                select Lineas from EstudiosGab
                where idconsulta = @consultId", new
                {
                    consultId
                }).FirstOrDefault();
                return Helper.LineFormat(lines);
            }
        }
    }
}