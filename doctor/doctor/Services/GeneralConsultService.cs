using Dapper;
using doctor.Database;
using doctor.Models;
using doctor.Models.Consults;
using doctor.Models.Consults.General;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using static doctor.Models.Consults.General.GeneralConsult;

namespace doctor.Services
{
    public class GeneralConsultService
    {
        private readonly string connection = "";

        public GeneralConsultService()
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
                        left join ConsultaGinecologa g on c.idconsulta = g.idconsulta
                        left join ConsultaObstetrica o on c.idconsulta = o.idconsulta
                        where idpaciente = @patientId and g.idconsulta is null and o.idconsulta is null 
                        order by c.Fecha desc", new
                {
                    patientId
                }).ToList();
            }
        }


        #region saveConsult
        public BasicResponse SaveConsult(int doctorId, GeneralConsult req)
        {
            var result = new BasicResponse();

            using (IDbConnection db = new SqlConnection(connection))
            {
                var cn = db;
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    var trans = transaction;
                    var now = DateTime.Now;

                    try
                    {
                        UpdatePatient(ref cn, ref trans, req.PatientConsult);

                        int affectedRowConsult = InsertConsult(ref cn, ref trans,
                            doctorId, req.PatientConsult.PatientId.Value, now, req);

                        int affectedRowRecipe = InsertComplement(ref cn, ref trans, "Recetas",
                            doctorId, req.PatientConsult.PatientId.Value, affectedRowConsult,
                            now, req.Treatments);

                        int affectedRowDiagnostics = InsertComplement(ref cn, ref trans, "Diagnosticos",
                            doctorId, req.PatientConsult.PatientId.Value, affectedRowConsult,
                            now, req.Diagnostics);

                        int affectedRowLab = InsertComplement(ref cn, ref trans, "EstudiosLab",
                            doctorId, req.PatientConsult.PatientId.Value, affectedRowConsult,
                            now, req.LaboratoryStudies);

                        int affectedRowCab = InsertComplement(ref cn, ref trans, "EstudiosGab",
                            doctorId, req.PatientConsult.PatientId.Value, affectedRowConsult,
                            now, req.CabinetStudies);

                        if (affectedRowConsult > 0 && affectedRowRecipe > 0 && affectedRowDiagnostics > 0
                            && affectedRowLab > 0 && affectedRowCab > 0)
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

        private int InsertConsult(ref IDbConnection db, ref IDbTransaction transaction, int doctorId,
            int patientId, DateTime now, GeneralConsult consult)
        {
            return db.QuerySingle<int>(@"
                    insert into Consultas
                    (idmedico, idpaciente, Peso, Altura, Temperatura, TensionArterial, TensionArterialB,
                    perimetroCefalico, FrecuenciaCardiaca, FrecuenciaRespiratoria, motivo,
                    SignosSintomas1, MedidasPreventivas, observaciones, Fecha, Pronostico)
                    values
                    (@doctorId, @patientId, @weight, @size, @temperature, @bloodA, @bloodB,
                    @headc, @heart, @breathing, @reason, @exploration, @measures, @observations,
                    @now, @pronostic);
                    select cast(scope_identity() as int)",
                    new
                    {
                        doctorId,
                        patientId,
                        weight = consult.BasicConsult.Weight,
                        size = consult.BasicConsult.Size,
                        temperature = consult.BasicConsult.Temperature,
                        bloodA = consult.BasicConsult.BloodPressure_A,
                        bloodB = consult.BasicConsult.BloodPressure_B,
                        headc = consult.HeadCircuference,
                        heart = consult.HeartRate,
                        breathing = consult.BreathingFrecuency,
                        reason = consult.ReasonForConsultation,
                        exploration = consult.PhysicalExploration,
                        measures = consult.PreventiveMeasures,
                        observations = consult.Observations,
                        now,
                        pronostic = consult.Prognostic != null ? string.Join("|", consult.Prognostic) : ""
                    }, transaction);
        }

        private int InsertComplement(ref IDbConnection db, ref IDbTransaction transaction, string tableName,
            int doctorId, int patientId, int consultId, DateTime now, List<Format> complement)
        {
            return db.Execute(@"
                    insert into " + tableName + @"
                    (idconsulta, idmedico, idpaciente, Fecha, Lineas)
                    values
                    (@consultId, @doctorId, @patientId, @now, @lines)",
                    new
                    {
                        consultId,
                        doctorId,
                        patientId,
                        now,
                        lines = SetLines(complement)
                    }, transaction);
        }
        #endregion


        public GeneralConsult GetConsultById(int consultId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();
                var consult = db.Query<ConsultasRepository>(@"
                        select * from Consultas where idconsulta = @consultId",
                        new
                        {
                            consultId
                        }).Select(x => new GeneralConsult
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
                            ConsultationDate = x.Fecha,
                            HeadCircuference = x.perimetroCefalico,
                            HeartRate = x.FrecuenciaCardiaca,
                            BreathingFrecuency = x.FrecuenciaRespiratoria,
                            ReasonForConsultation = x.motivo,
                            PhysicalExploration = x.SignosSintomas1,
                            PreventiveMeasures = x.MedidasPreventivas,
                            Observations = x.observaciones,
                            _Prognostic = x.Pronostico
                        }).FirstOrDefault();
                if (consult != null)
                {
                    if (consult.BasicConsult.Size % 1 == 0)
                        consult.BasicConsult.Size /= 100;
                    if (consult.BasicConsult.Weight > 0 && consult.BasicConsult.Size > 0)
                        consult.BasicConsult.Mass = consult.BasicConsult.Weight / (consult.BasicConsult.Size * consult.BasicConsult.Size);

                    consult.Diagnostics = new DiagnosticService().GetDiagnosticsByConsult(consultId);
                    consult.Treatments = new TreatmentService().GetTreatmentsByConsult(consultId);
                    consult.CabinetStudies = new StudiesService().GetCabinetStudies(consultId);
                    consult.LaboratoryStudies = new StudiesService().GetLaboratoryStudies(consultId);

                    consult.Prognostic = consult._Prognostic != null ?
                        consult._Prognostic.Split('|', (char)StringSplitOptions.RemoveEmptyEntries).ToList()
                        : new List<string>();
                }

                return consult;
            }
        }


        /*
         * [new]:name=x|x:name=x|x
         * 
         * si lines empieza con '[new]=' quiere decir que se trata del nuevo
         * formato, ya que la forma vieja no guarda correctamente los datos
         */
        private string SetLines(List<Format> lines)
        {
            string result = "";
            if (lines != null && lines.Count > 0)
            {
                result = "[new]";
                foreach (var sr in lines)
                {
                    result += $">{sr.Name}=";
                    foreach (var s in sr.Studies)
                    {
                        result += $"{s}|";
                    }
                    result = result.Remove(result.Length - 1);
                }
            }

            return result;
        }
    }
}