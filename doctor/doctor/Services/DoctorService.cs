using Dapper;
using doctor.Database;
using doctor.Models;
using doctor.Models.Doctor;
using doctor.Models.Doctor.Req;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace doctor.Services
{
    public class DoctorService
    {
        private readonly string connection = "";

        public DoctorService()
        {
            connection = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;
        }

        public BasicInfo GetBasicInformation(int doctorId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                return db.Query<BasicInfo>(@"
                        select
                        (r.Nombre + ' '  + r.Apellido) as Name,
                        r.emeci as EMECI
                        from Registro r
                        inner join Medico m on r.idRegistro = m.IdRegistro
                        where m.Idmedico = @doctorId",
                        new
                        {
                            doctorId
                        }).FirstOrDefault();
            }
        }

        public BasicResponse ChangePassword(int doctorId, ChangePasswordReq req)
        {
            var result = new BasicResponse();

            using(IDbConnection db = new SqlConnection(connection))
            {
                db.Open();
                var register = db.Query<RegistroRepository>(@"
                        select r.*
                        from Registro r
                        inner join Medico m on r.idRegistro = m.IdRegistro
                        where m.Idmedico = @doctorId",
                        new
                        {
                            doctorId
                        }).FirstOrDefault();
                if (register != null)
                {
                    if (register.Clave == req.CurrentPassword)
                    {
                        try
                        {
                            db.Execute(@"
                                update Registro
                                set clave = @newPassword
                                where IdRegistro = @registerId",
                                new
                                {
                                    newPassword = req.NewPassword,
                                    registerId = register.IdRegistro
                                });
                            result.IsSuccess = true;

                        }
                        catch (DBConcurrencyException ex)
                        {
                            result.Error = ex.Message;
                            Log.Write(ex.Message);
                        }
                    }
                    else result.Error = "Error, la contraseña actual es incorrecta";
                }
                else result.Error = "Error al obtener el doctor";
            }
            return result;
        }


        #region register
        public BasicResponse Register(RegisterReq req)
        {
            var result = new BasicResponse();

            using (IDbConnection db = new SqlConnection(connection))
            {
                var cn = db;
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    var trans = transaction;

                    try
                    {
                        int affectedRowRegister = InsertRegister(ref cn, ref trans, req);
                        int affectedRowDoctor = 0;
                        if (affectedRowRegister > 0)
                            affectedRowDoctor = InsertDoctor(ref cn, ref trans, affectedRowRegister, req);

                        if (affectedRowRegister > 0 && affectedRowDoctor > 0)
                        {
                            transaction.Commit();
                            result.IsSuccess = true;
                        }
                    }
                    catch (DBConcurrencyException ex)
                    {
                        result.Error = ex.Message;
                        Log.Write(ex.Message);
                    }
                }
            }
            return result;
        }

        private int InsertRegister(ref IDbConnection db, ref IDbTransaction transaction, RegisterReq req)
        {
            return db.QuerySingle<int>(@"
                    insert into Registro
                    (Nombre, Apellido, Colonia, Domicilio, Emails, Telefono, TelefonoCel,
                    CodigoPostal, idEstado, idCiudad, IdPais, FechaRegistro, Tipo, Status, CURP)
                    values
                    (@name, @lastName, @colony, @address, @email, @phone, @cellPhone,
                    @cp, @stateId, @cityId, @countryId, @registrationDate, @type, @status, @curp);
                    select cast(scope_identity() as int)",
                    new
                    {
                        name = req.Name,
                        lastName = req.LastName,
                        colony = req.Colony,
                        address = req.Address,
                        email = req.Email,
                        phone= req.Phone,
                        cellPhone = req.CellPhone,
                        cp = req.PostalCode,
                        stateId = req.State,
                        cityId = int.TryParse(req.City, out int intValue) ? intValue : (int?)null,
                        countryId = "MX",
                        registrationDate = DateTime.Now,
                        type = "M",
                        status = "V",
                        curp = req.CURP
                    }, transaction);
        }

        private int InsertDoctor(ref IDbConnection db, ref IDbTransaction transaction, int registerId, RegisterReq req)
        {
            return db.Execute(@"
                    insert into Medico
                    (IdRegistro, RFC, TelefonoConsultorio, DomicilioConsultorio, CertCMCP,
                    CedulaEspecialidad, NoRegSSA, AgrupacionLocal, AgrupacionNacional,
                    UniversidadEspecialidad, CedulaProfesional, HospitalResidenciaPediatra)
                    values
                    (@registerId, @rfc, @officePhone, @officeAddress, @cmcp, @certificate,
                    @ssa, @school, @grouping, @university, @sep, @hospital)",
                    new
                    {
                        registerId,
                        rfc = req.RFC,
                        officePhone = req.OfficePhone,
                        officeAddress = req.OfficeAddress,
                        cmcp = req.NoCertification_CMCP,
                        certificate = req.SpecialtyCertificate,
                        ssa = req.NoSSA,
                        school = req.NameStateSchool,
                        grouping = req.NameStateGrouping,
                        university = req.UniversitySpecialty,
                        sep = req.NoSEP_ProfessionalCertificate,
                        hospital = req.ProfessionalResidenceHospital,
                    }, transaction);
        }
        #endregion


        #region update register
        public BasicResponse UpdateRegister(int doctorId, RegisterReq req)
        {
            var result = new BasicResponse();

            using (IDbConnection db = new SqlConnection(connection))
            {
                var cn = db;
                db.Open();

                var registerId = db.Query<int>(@"
                    select IdRegistro from Medico where Idmedico = @doctorId",
                    new { doctorId }).FirstOrDefault();

                if (registerId == 0)
                {
                    result.Error = "Error al encontrar el registro del doctor";
                    return result;
                }

                using (var transaction = db.BeginTransaction())
                {
                    var trans = transaction;
                    try
                    {
                        UpdateDoctor(ref cn, ref trans, doctorId, req);
                        UpdateRegister(ref cn, ref trans, registerId, req);

                        transaction.Commit();
                        result.IsSuccess = true;
                    } catch (DBConcurrencyException ex)
                    {
                        result.Error = ex.Message;
                        Log.Write(ex.Message);
                    }
                }
            }
            return result;
        }

        private void UpdateDoctor(ref IDbConnection db, ref IDbTransaction transaction, int doctorId, RegisterReq req)
        {
            db.Execute(@"
                update Medico
                set RFC = @rfc,
                TelefonoConsultorio = @officePhone,
                DomicilioConsultorio = @officeAddress,
                CertCMCP = @cmcp,
                CedulaEspecialidad = @certificate,
                NoRegSSA = @ssa,
                AgrupacionLocal = @school,
                AgrupacionNacional = @grouping,
                UniversidadEspecialidad = @university,
                CedulaProfesional = @sep,
                HospitalResidenciaPediatra = @hospital
                where Idmedico = @doctorId",
                new
                {
                    rfc = req.RFC,
                    officePhone = req.OfficePhone,
                    officeAddress = req.OfficeAddress,
                    cmcp = req.NoCertification_CMCP,
                    certificate = req.SpecialtyCertificate,
                    ssa = req.NoSSA,
                    school = req.NameStateSchool,
                    grouping = req.NameStateGrouping,
                    university = req.UniversitySpecialty,
                    sep = req.NoSEP_ProfessionalCertificate,
                    hospital = req.ProfessionalResidenceHospital,
                    doctorId
                }, transaction);
        }

        private void UpdateRegister(ref IDbConnection db, ref IDbTransaction transaction, int registerId, RegisterReq req)
        {
            db.Execute(@"
                update Registro
                set Nombre = @name,
                Apellido = @lastName,
                Colonia = @colony,
                Domicilio = @address,
                Emails = @email,
                Telefono = @phone,
                TelefonoCel = @cellPhone,
                CodigoPostal = @cp,
                idEstado = @stateId,
                idCiudad = @cityId,
                CURP = @curp
                where idRegistro = @registerId",
                new
                {
                    name = req.Name,
                    lastName = req.LastName,
                    colony = req.Colony,
                    address = req.Address,
                    email = req.Email,
                    phone = req.Phone,
                    cellPhone = req.CellPhone,
                    cp = req.PostalCode,
                    stateId = req.State,
                    cityId = int.TryParse(req.City, out int intValue) ? intValue : (int?)null,
                    curp = req.CURP,
                    registerId
                }, transaction);
        }
        #endregion


        public RegisterInformation GetRegisterInformation(int doctorId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                return db.Query<RegisterInformation>(@"
                        select
                        m.RFC,
                        m.TelefonoConsultorio as OfficePhone,
                        m.DomicilioConsultorio as OfficeAddress,
                        m.CertCMCP as NoCertification_CMCP,
                        m.CedulaEspecialidad as SpecialtyCertificate,
                        m.NoRegSSA as NoSSA,
                        m.AgrupacionLocal as NameStateSchool,
                        m.AgrupacionNacional as NameStateGrouping,
                        m.UniversidadEspecialidad as UniversitySpecialty,
                        m.CedulaProfesional as NoSEP_ProfessionalCertificate,
                        m.HospitalResidenciaPediatra as ProfessionalResidenceHospital,
                        r.Nombre as Name,
                        r.Apellido as LastName,
                        r.Colonia as Colony,
                        r.Domicilio as Address,
                        r.Emails as Email,
                        r.Telefono as Phone,
                        r.TelefonoCel as CellPhone,
                        r.CodigoPostal as PostalCode,
                        r.idEstado as State,
                        r.idCiudad as City,
                        r.CURP
                        from Registro r
                        inner join Medico m on r.idRegistro = m.IdRegistro
                        where m.Idmedico = @doctorId",
                        new
                        {
                            doctorId
                        }).FirstOrDefault();
            }
        }

        public IEnumerable<DoctorsList> GetListOfDoctorByAssociation(int associationId)
        {
            IEnumerable<DoctorsList> result = Enumerable.Empty<DoctorsList>();
            using (IDbConnection db = new SqlConnection(connection))
            {
                var doctors = db.Query<int>(@"
                        select DoctorId from DoctorsByAssociation
                        where AssociationId = @associationId", new { associationId }).ToArray();
                if (doctors.Length > 0)
                {
                    return db.Query<DoctorsList>(@"
                        select
                        (r.Nombre + ' '  + r.Apellido) as Name,
                        m.Idmedico as DoctorId
                        from Registro r
                        inner join Medico m on r.idRegistro = m.IdRegistro
                        where m.Idmedico in @doctorsId",
                        new
                        {
                            doctorsId = doctors
                        }).ToList();
                }
            }
            return result;
        }
    }
}