using Dapper;
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
                        select idconsulta as Id,
                        Fecha as ConsultationDate from Consultas
                        where idpaciente = @patientId
                        order by Fecha desc", new
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

                        int affectedRowRecipe = InsertRecipe(ref cn, ref trans,
                            doctorId, req.PatientConsult.PatientId.Value, affectedRowConsult,
                            now, req.Treatments);

                        int affectedRowDiagnostics = InsertDiagnostics(ref cn, ref trans,
                            doctorId, req.PatientConsult.PatientId.Value, affectedRowConsult,
                            now, req.Diagnostics);

                        int affectedRowLab = InsertLabStudies(ref cn, ref trans,
                            doctorId, req.PatientConsult.PatientId.Value, affectedRowConsult,
                            now, req.LaboratoryStudies);

                        int affectedRowCab = InsertCabStudies(ref cn, ref trans,
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

        private int InsertConsult(ref IDbConnection db,
            ref IDbTransaction transaction, int doctorId,
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
                    @now, @pronostic)",
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

        private int InsertRecipe(ref IDbConnection db,
            ref IDbTransaction transaction, int doctorId,
            int patientId, int consultId, DateTime now,
            List<string> treatments)
        {
            return db.QuerySingle<int>(@"
                    insert into Recetas
                    (idconsulta, idmedico, idpaciente, Fecha, Lineas)
                    values
                    (@consultId, @doctorId, @patientId, @now, @lines)",
                    new
                    {
                        consultId,
                        doctorId,
                        patientId,
                        now,
                        lines = SetLines(treatments)
                    }, transaction);
        }

        private int InsertDiagnostics(ref IDbConnection db,
            ref IDbTransaction transaction, int doctorId,
            int patientId, int consultId, DateTime now,
            List<string> diagnostics)
        {
            return db.QuerySingle<int>(@"
                    insert into Diagnosticos
                    (idconsulta, idmedico, idpaciente, Fecha, Lineas)
                    values
                    (@consultId, @doctorId, @patientId, @now, @lines)",
                    new
                    {
                        consultId,
                        doctorId,
                        patientId,
                        now,
                        lines = SetLines(diagnostics)
                    }, transaction);
        }

        private int InsertLabStudies(ref IDbConnection db,
            ref IDbTransaction transaction, int doctorId,
            int patientId, int consultId, DateTime now,
            List<string> laboratoryStudies)
        {
            return db.QuerySingle<int>(@"
                    insert into EstudiosLab
                    (idconsulta, idmedico, idpaciente, Fecha, Lineas)
                    values
                    (@consultId, @doctorId, @patientId, @now, @lines)",
                    new
                    {
                        consultId,
                        doctorId,
                        patientId,
                        now,
                        lines = SetLines(laboratoryStudies)
                    }, transaction);
        }

        private int InsertCabStudies(ref IDbConnection db,
            ref IDbTransaction transaction, int doctorId,
            int patientId, int consultId, DateTime now,
            List<string> cabinetStudies)
        {
            return db.QuerySingle<int>(@"
                    insert into EstudiosGab
                    (idconsulta, idmedico, idpaciente, Fecha, Lineas)
                    values
                    (@consultId, @doctorId, @patientId, @now, @lines)",
                    new
                    {
                        consultId,
                        doctorId,
                        patientId,
                        now,
                        lines = SetLines(cabinetStudies)
                    }, transaction);
        }
        #endregion


        private string SetLines(List<string> lines)
        {
            int count = 0;
            string l = "";
            if (lines != null)
            {
                foreach (var sr in lines)
                {
                    if (sr.Trim() != "")
                    {
                        l += $"{count++:0#}{sr}\r\n";
                    }
                }
            }
            return l;
        }
    }
}