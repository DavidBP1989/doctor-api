using Dapper;
using doctor.Database;
using doctor.Models;
using doctor.Models.Consults;
using doctor.Models.Consults.General;
using doctor.Models.Consults.Gynecology;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace doctor.Services
{
    public class GynecologyConsultService
    {
        private readonly string connection = "";

        public GynecologyConsultService()
        {
            connection = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;
        }

        public IEnumerable<ConsultationDates> GetPreviousConsultDates(int patientId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();
                return db.Query<ConsultationDates>(@"
                        select c.idconsulta as Id,
                        c.Fecha as ConsultationDate
                        from Consultas c
                        right join ConsultaGinecologa g on c.idconsulta = g.idconsulta
                        where c.idpaciente = @patientId
                        order by c.Fecha desc", new
                {
                    patientId
                }).ToList();
            }
        }


        #region saveConsult
        public BasicResponse SaveConsult(int doctorId, GynecologyConsult req)
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
                        UpdatePatient(ref cn, ref trans, req.PatientConsult);

                        int affectedRowConsult = InsertConsult(ref cn, ref trans,
                            doctorId, req.PatientConsult.PatientId.Value, req.BasicConsult);

                        int affectedRowGyencologyConsult = InsertGynecologyConsult(ref cn, ref trans, affectedRowConsult, req);

                        if (affectedRowConsult > 0 && affectedRowGyencologyConsult > 0)
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

        private void UpdatePatient(ref IDbConnection db, ref IDbTransaction transaction, PatientConsult patient)
        {
            db.Execute(@"
                update Paciente
                set AlergiaMedicina = @allergies,
                AlergiaOtros = @others,
                Patologia = @pathologies
                where idPaciente = @patientId", new
            {
                allergies = patient.Allergies,
                others = patient.Reserved,
                pathologies = patient.RelevantPathologies,
                patientId = patient.PatientId
            }, transaction);
        }

        private int InsertConsult(ref IDbConnection db, ref IDbTransaction transaction,
            int doctorId, int patientId, BasicConsult consult)
        {
            return db.QuerySingle<int>(@"
                    insert into Consultas
                    (idmedico, idpaciente, Peso, Altura, Temperatura,
                    TensionArterial, TensionArterialB, motivo, Fecha)
                    values
                    (@doctorId, @patientId, @weight, @size, @temperature,
                    @bloodA, @bloodB, @reason, @now);
                    select cast(scope_identity() as int)",
                    new
                    {
                        doctorId,
                        patientId,
                        weight = consult.Weight,
                        size = consult.Size,
                        temperature = consult.Temperature,
                        bloodA = consult.BloodPressure_A,
                        bloodB = consult.BloodPressure_B,
                        reason = consult.ReasonForConsultation,
                        now = DateTime.Now
                    }, transaction);
        }

        private int InsertGynecologyConsult(ref IDbConnection db,  ref IDbTransaction transaction, int generalConsultId, GynecologyConsult consult)
        {
            bool hasPartner = consult.Partner.HasAPartner;
            return db.Execute(@"
                    insert into ConsultaGinecologa
                    (idconsulta, FechaUltimaMestruacion, Gestas, ParaGestas, Cesareas, abortos,
                    RecienNacidosVivos, mortinatos, EdadInicioVidaSexual, menacma, oligonorrea,
                    Proiomenorrea, Hipermenorrea, Dismenorrea, Dispareunia, Leucorrea, Lactorrea,
                    Amenorrea, Metrorragia, Otros, OtrosEspecifique, TienePareja, SexoPareja,
                    EstadoCivilPareja, GrupoRHPareja, FechaNacimientoPareja, OcupacionPareja,
                    TelefonoPareja, nombrePareja, edadPareja)
                    values
                    (@consultId, @lastMenstruation, @gestas, @paragestas, @cesareans, @abortions,
                    @newlyBorn, @stillbirth, @ageActiveSexual, @menacma, @oligomenorrea,
                    @proiomenorrea, @hipermenorrea, @dismenorrea, @dispareunia, @leucorrea, @lactorrea,
                    @amenorrea, @metrorragia, @others, @specifyOthers, @hasPartner, @sex, @status,
                    @rh, @birthdate, @occupation, @phone, @name, @age)",
                    new
                    {
                        consultId = generalConsultId,
                        lastMenstruation = consult.LastMenstruationDate,
                        gestas = consult.Gestas,
                        paragestas = consult.Paragestas,
                        cesareans = consult.Cesareans,
                        abortions = consult.Abortions,
                        newlyBorn = consult.NewlyBorn,
                        stillbirth = consult.Stillbirth,
                        ageActiveSexual = consult.AgeOfOnsetOfActiveSexualLife,
                        menacma = consult.Menacma,
                        oligomenorrea = consult.Checkbox.Oligomenorrea,
                        proiomenorrea = consult.Checkbox.Proiomenorrea,
                        hipermenorrea = consult.Checkbox.Hipermenorrea,
                        dismenorrea = consult.Checkbox.Dismenorrea,
                        dispareunia = consult.Checkbox.Dispareunia,
                        leucorrea = consult.Checkbox.Leucorrea,
                        lactorrea = consult.Checkbox.Lactorrea,
                        amenorrea = consult.Checkbox.Amenorrea,
                        metrorragia = consult.Checkbox.Metrorragia,
                        others = consult.Checkbox.Others,
                        specifyOthers = consult.SpecifyOthers,
                        hasPartner,
                        sex = hasPartner ? consult.Partner.Sex : null,
                        status = hasPartner ? consult.Partner.MaritalStatus : null,
                        rh = hasPartner ? consult.Partner.GroupRH : null,
                        birthdate = hasPartner ? consult.Partner.BirthDate : null,
                        occupation = hasPartner ? consult.Partner.Occupation : null,
                        phone = hasPartner ? consult.Partner.Phone : null,
                        name = hasPartner ? consult.Partner.Name : null,
                        age = hasPartner ? consult.Partner.Age : null
                    }, transaction);
        }
        #endregion


        public GynecologyConsult GetConsultById(int consultId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();
                var consult = db.Query<ConsultasRepository, ConsultaGinecologaRepository, ConsultasRepository>(@"
                        select * from Consultas c
                        inner join ConsultaGinecologa g on c.idconsulta = g.idconsulta
                        where c.idconsulta = @consultId",
                        (general, gynecology) =>
                        {
                            general.GynecologyConsult = gynecology;
                            return general;
                        },
                        new { consultId }, null, false, splitOn: "idconsultaginecologa")
                    .Select(x => new GynecologyConsult
                    {
                        BasicConsult = new BasicConsult
                        {
                            Weight = x.Peso,
                            Size = x.Altura,
                            Mass = 0,
                            Temperature = x.Temperatura,
                            BloodPressure_A = x.TensionArterial,
                            BloodPressure_B = x.TensionArterialB
                        },
                        PatientConsult = new PatientConsult
                        {
                            PatientId = x.idpaciente
                        },
                        LastMenstruationDate = x.GynecologyConsult.FechaUltimaMestruacion,
                        Gestas = (int)x.GynecologyConsult.Gestas,
                        Paragestas = (int)x.GynecologyConsult.ParaGestas,
                        Cesareans = (int)x.GynecologyConsult.Cesareas,
                        Abortions = (int)x.GynecologyConsult.abortos,
                        NewlyBorn = (int)x.GynecologyConsult.RecienNacidosVivos,
                        Stillbirth = (int)x.GynecologyConsult.mortinatos,
                        AgeOfOnsetOfActiveSexualLife = (int)x.GynecologyConsult.EdadInicioVidaSexual,
                        Menacma = x.GynecologyConsult.menacma,
                        Checkbox = new Options
                        {
                            Oligomenorrea = x.GynecologyConsult.oligonorrea.Value,
                            Proiomenorrea = x.GynecologyConsult.Proiomenorrea.Value,
                            Hipermenorrea = x.GynecologyConsult.Hipermenorrea.Value,
                            Dismenorrea = x.GynecologyConsult.Dismenorrea.Value,
                            Dispareunia = x.GynecologyConsult.Dispareunia.Value,
                            Leucorrea = x.GynecologyConsult.Leucorrea.Value,
                            Lactorrea = x.GynecologyConsult.Lactorrea.Value,
                            Amenorrea = x.GynecologyConsult.Amenorrea.Value,
                            Metrorragia = x.GynecologyConsult.Metrorragia.Value,
                            Others = x.GynecologyConsult.Otros.Value
                        },
                        SpecifyOthers = x.GynecologyConsult.OtrosEspecifique,
                        Partner = new Partner
                        {
                            HasAPartner = x.GynecologyConsult.TienePareja.Value,
                            Name = x.GynecologyConsult.nombrePareja,
                            Sex = x.GynecologyConsult.SexoPareja,
                            BirthDate = x.GynecologyConsult.FechaNacimientoPareja,
                            GroupRH = x.GynecologyConsult.GrupoRHPareja,
                            Age = x.GynecologyConsult.edadPareja,
                            MaritalStatus = x.GynecologyConsult.EstadoCivilPareja,
                            Occupation = x.GynecologyConsult.OcupacionPareja,
                            Phone = x.GynecologyConsult.TelefonoPareja
                        }
                    }).FirstOrDefault();


                if (consult != null)
                {
                    if (consult.BasicConsult.Size % 1 == 0)
                        consult.BasicConsult.Size /= 100;
                    if (consult.BasicConsult.Weight > 0 && consult.BasicConsult.Size > 0)
                    {
                        double mass = (double)(consult.BasicConsult.Weight / (consult.BasicConsult.Size * consult.BasicConsult.Size));
                        consult.BasicConsult.Mass = (float)Math.Round(mass, 2);
                    }

                    consult.MenarcaAge =
                        new PatientService().GetMenarcaAgeByPatientId(consult.PatientConsult.PatientId.Value);
                }

                return consult;
            }
        }
    }
}