using Dapper;
using doctor.Models.Doctor;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace doctor.Services
{
    public class GeneralService
    {
        private readonly string connection = "";

        public GeneralService()
        {
            connection = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;
        }

        public List<States> GetStates()
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                return db.Query<States>(@"
                    select
                    idEstado as Id,
                    Nombre as Name
                    from Estados where IdPais = 'MX'").ToList();
            }
        }

        public List<Cities> GetCities(string stateId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                return db.Query<Cities>(@"
                    select
                    idciudad as Id,
                    Nombre as Name
                    from Ciudades where idEstado = @stateId", new
                {
                    stateId
                }).ToList();
            }
        }

        public ForgotPassword ForgotPassword(string emeci)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                return db.Query<ForgotPassword>(@"
                    select Emails as Email,
                    clave as Password,
                    (Nombre + ' ' + Apellido) as DoctorName
                    from Registro
                    where Emeci = @emeci and Status = 'V'",
                    new
                    {
                        emeci
                    }).FirstOrDefault();
            }
        }

        public List<MedicalSpecialties> GetMedicalSpecialties()
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                return db.Query<MedicalSpecialties>(@"select * from EspecialidadMedica").ToList();
            }
        }
    }
}