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

        public ForgotPassword ForgotPassword(string email)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                return db.Query<ForgotPassword>($@"
                    select emails as Email,
                    clave as Password,
                    (nombre + ' ' + apellido) as DoctorName
                    from registro
                    where emails like '%{email}%' and
                    status = 'V' and
                    tipo = 'M'").FirstOrDefault();
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