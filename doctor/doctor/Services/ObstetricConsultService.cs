using Dapper;
using doctor.Database;
using doctor.Models;
using doctor.Models.Consults;
using doctor.Models.Consults.Obstetric;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace doctor.Services
{
    public class ObstetricConsultService
    {
        private readonly string connection = "";

        public ObstetricConsultService()
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
                        right join ConsultaObstetrica g on c.idconsulta = g.idconsulta
                        where c.idpaciente = @patientId
                        order by c.Fecha desc", new
                {
                    patientId
                }).ToList();
            }
        }


        #region saveConsult
        public BasicResponse SaveConsult(int doctorId, ObstetricConsult req)
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
                            doctorId, req.PatientConsult.PatientId.Value, req.BasicConsult);

                        var generalConsult = new GeneralConsultService();

                        int affectedRowRecipe = generalConsult.InsertComplement(ref cn, ref trans, "Recetas",
                            doctorId, req.PatientConsult.PatientId.Value, affectedRowConsult,
                            now, req.Treatments);

                        int affectedRowDiagnostics = generalConsult.InsertComplement(ref cn, ref trans, "Diagnosticos",
                            doctorId, req.PatientConsult.PatientId.Value, affectedRowConsult,
                            now, req.Diagnostics);

                        int affectedRowLab = generalConsult.InsertComplement(ref cn, ref trans, "EstudiosLab",
                            doctorId, req.PatientConsult.PatientId.Value, affectedRowConsult,
                            now, req.LaboratoryStudies);

                        int affectedRowCab = generalConsult.InsertComplement(ref cn, ref trans, "EstudiosGab",
                            doctorId, req.PatientConsult.PatientId.Value, affectedRowConsult,
                            now, req.CabinetStudies);

                        int affectedRowObstetricConsult = InsertObstetricConsult(ref cn, ref trans, affectedRowConsult, req);

                        if (affectedRowConsult > 0 && affectedRowRecipe > 0 && affectedRowDiagnostics > 0
                            && affectedRowLab > 0 && affectedRowCab > 0 && affectedRowObstetricConsult > 0)
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
                    TensionArterial, TensionArterialB, motivo, SignosSintomas1,
                    MedidasPreventivas, observaciones, Fecha)
                    values
                    (@doctorId, @patientId, @weight, @size, @temperature,
                    @bloodA, @bloodB, @reason, @exploration, @measures, @observations, @now);
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
                        exploration = consult.PhysicalExploration,
                        measures = consult.PreventiveMeasures,
                        observations = consult.Observations,
                        now = DateTime.Now
                    }, transaction);
        }

        private int InsertObstetricConsult(ref IDbConnection db, ref IDbTransaction transaction, int generalConsultId, ObstetricConsult consult)
        {
            bool isParturation = consult.Parturition == 1;

            return db.Execute(@"
                    insert into ConsultaObstetrica
                    (idconsulta, noembarazo, activaSexualmente, abortos, FechaUltmoParto, PrimerDiaUltimaMestruacuion,
                    ToxemiasPrevias, EspecifiqueToxemias, Partos, CesareasPrevia, UsoDeForceps,
                    Motinatos, RMVivos, EmbarazoEtopicos, EmbrazoEtopicoExplique, EmbrazosComplicadosPrevios,
                    EmbarazosComplicadosExplique, NoComplicacionesPertinales, ComplicacionesPerinatalesExplique,
                    NoEmbrazosAnormales, EmbarazosAnormalesExplique, Observaciones, FU, FCF, CC, CA,
                    LF, DSP, Posicion, Presentacion, siuacuion, Actitud, MovimientosFetales, PesoAproxProducto,
                    TA, FCM, Edema, SeHizoUf, ultrasonido, exploracionFisica, TipoDistocia,
                    EspecifiqueTipoDistocia, MotivoDistocia, EspecifiqueMotivoDistocia)
                    values
                    (@consultId, @pregnancyNumber, @sexually, @abortions, @lastParturation, @firstMenstruation,
                    @toxemias, @sToxemias, @parturations, @pCesareans, @forceps, @stillbirth,
                    @newborn, @ectopic, @sEctopic, @pPregnancy, @sPPregnancy, @perinatal, @sPerinatal,
                    @abnormal, @sAbnormal, @observations, @fu, @fcf, @cc, @ca, @lf, @dsp, @position,
                    @presentation, @situtation, @attitude, @fetal, @weight, @ta, @fcm, @edema, @uf,
                    @ultrasound, @exploration, @dystociaT, @sDystociaT, @dystociaM, @sDystociaM)",
                    new
                    {
                        consultId = generalConsultId,
                        pregnancyNumber = consult.PregnancyNumber,
                        sexually = consult.SexuallyActive,
                        abortions = consult.Abortions,
                        lastParturation = consult.LastParturitionDate,
                        firstMenstruation = consult.FirstDayOfLastMenstruation,
                        toxemias = (byte?)(consult.PreviousToxemias ? 1 : 0),
                        sToxemias = consult.PreviousToxemias ? consult.SpecifyToxemias : null,
                        parturations = consult.Parturition,
                        pCesareans = consult.PreviousCesarean,
                        forceps = consult.Forceps,
                        stillbirth = consult.Stillbirths,
                        newborn = consult.NewbornAlive,
                        ectopic = (byte?)(consult.EctopicPregnancies ? 1 : 0),
                        sEctopic = consult.EctopicPregnancies ? consult.SpecifyEctopicPregnancies : null,
                        pPregnancy = (byte?)(consult.PreviousPregnacyComplications ? 1 : 0),
                        sPPregnancy = consult.PreviousPregnacyComplications ? consult.SpecifyPreviousPregnacyComplications : null,
                        perinatal = (byte?)(consult.PerinatalComplications ? 1 : 0),
                        sPerinatal = consult.PerinatalComplications ? consult.SpecifyPerinatalComplications : null,
                        abnormal = (byte?)(consult.AbnormalPregnancies ? 1 : 0),
                        sAbnormal = consult.AbnormalPregnancies ? consult.SpecifyAbnormalPregnancies : null,
                        observations = consult.Observations,
                        fu = consult.PregnancyControl.FU,
                        fcf = consult.PregnancyControl.FCF,
                        cc = consult.PregnancyControl.CC,
                        ca = consult.PregnancyControl.CA,
                        lf = consult.PregnancyControl.LF,
                        dsp = consult.PregnancyControl.DBP,
                        position = consult.PregnancyControl.Position,
                        presentation = consult.PregnancyControl.Presentation,
                        situtation = consult.PregnancyControl.Situtation,
                        attitude = consult.PregnancyControl.Attitude,
                        fetal = consult.PregnancyControl.FetalMovements,
                        weight = consult.PregnancyControl.ApproximateProductWeight,
                        ta = consult.PregnancyControl.TA,
                        fcm = consult.PregnancyControl.FCM,
                        edema = consult.PregnancyControl.Edema,
                        uf = consult.PregnancyControl.MadeUf,
                        ultrasound = consult.PregnancyControl.Ultrasound,
                        exploration = consult.PregnancyControl.PhysicalExploration,
                        dystociaT = isParturation ? (byte ?)consult.DystociaType : null,
                        sDystociaT = (isParturation && consult.DystociaType == 2) ? consult.SpecifyDystociaType : null,
                        dystociaM = isParturation ? (byte?)consult.DystociaReason : null,
                        sDystociaM = (isParturation && consult.DystociaReason == 3) ? consult.SpecifyDystociaReason : null
                    }, transaction);
        }
        #endregion


        public ObstetricConsult GetConsultById(int consultId)
        {
            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                var consult = db.Query<ConsultasRepository, ConsultaObstetricaRepository, ConsultasRepository>(@"
                        select * from Consultas c
                        inner join ConsultaObstetrica g on c.idconsulta = g.idconsulta
                        where c.idconsulta = @consultId",
                        (general, obstetric) =>
                        {
                            general.ObstetricConsult = obstetric;
                            return general;
                        },
                        new { consultId }, null, false, splitOn: "IdConsultaObstetrica")
                    .Select(x => new ObstetricConsult
                    {
                        BasicConsult = new BasicConsult
                        {
                            Weight = x.Peso,
                            Size = x.Altura,
                            Mass = 0,
                            Temperature = x.Temperatura,
                            BloodPressure_A = x.TensionArterial,
                            BloodPressure_B = x.TensionArterialB,
                            ReasonForConsultation = x.motivo,
                            PhysicalExploration = x.SignosSintomas1,
                            PreventiveMeasures = x.MedidasPreventivas,
                            Observations = !string.IsNullOrEmpty(x.ObstetricConsult.Observaciones) ?
                            x.ObstetricConsult.Observaciones : x.observaciones
                        },
                        PatientConsult = new PatientConsult
                        {
                            PatientId = x.idpaciente
                        },
                        PregnancyNumber = x.ObstetricConsult.noembarazo.Value,
                        SexuallyActive = x.ObstetricConsult.activaSexualmente.Value,
                        LastParturitionDate = x.ObstetricConsult.FechaUltmoParto,
                        FirstDayOfLastMenstruation = x.ObstetricConsult.PrimerDiaUltimaMestruacuion,
                        PreviousToxemias = x.ObstetricConsult.ToxemiasPrevias.Value == 1,
                        SpecifyToxemias = x.ObstetricConsult.EspecifiqueToxemias,
                        Parturition = x.ObstetricConsult.Partos.Value,
                        DystociaType = x.ObstetricConsult.TipoDistocia ?? 0,
                        SpecifyDystociaType = x.ObstetricConsult.EspecifiqueTipoDistocia,
                        DystociaReason = x.ObstetricConsult.MotivoDistocia ?? 0,
                        SpecifyDystociaReason = x.ObstetricConsult.EspecifiqueMotivoDistocia,
                        PreviousCesarean = x.ObstetricConsult.CesareasPrevia.Value,
                        Forceps = x.ObstetricConsult.UsoDeForceps.Value,
                        Stillbirths = x.ObstetricConsult.Motinatos.Value,
                        NewbornAlive = x.ObstetricConsult.RMVivos.Value,
                        EctopicPregnancies = x.ObstetricConsult.EmbarazoEtopicos == 1,
                        SpecifyEctopicPregnancies = x.ObstetricConsult.EmbrazoEtopicoExplique,
                        PreviousPregnacyComplications = x.ObstetricConsult.EmbrazosComplicadosPrevios == 1,
                        SpecifyPreviousPregnacyComplications = x.ObstetricConsult.EmbarazosComplicadosExplique,
                        PerinatalComplications = x.ObstetricConsult.NoComplicacionesPertinales == 1,
                        SpecifyPerinatalComplications = x.ObstetricConsult.ComplicacionesPerinatalesExplique,
                        AbnormalPregnancies = x.ObstetricConsult.NoEmbrazosAnormales == 1,
                        SpecifyAbnormalPregnancies = x.ObstetricConsult.EmbarazosAnormalesExplique,
                        //Observations = x.ObstetricConsult.Observaciones,
                        PregnancyControl = new PregnancyControl
                        {
                            FU = x.ObstetricConsult.FU.Value,
                            FCF = x.ObstetricConsult.FCF.Value,
                            CC = x.ObstetricConsult.CC.Value,
                            CA = x.ObstetricConsult.CA.Value,
                            LF = x.ObstetricConsult.LF.Value,
                            DBP = x.ObstetricConsult.DSP.Value,
                            Position = x.ObstetricConsult.Posicion,
                            Presentation = x.ObstetricConsult.Presentacion,
                            Situtation = x.ObstetricConsult.siuacuion,
                            Attitude = x.ObstetricConsult.Actitud,
                            FetalMovements = x.ObstetricConsult.MovimientosFetales,
                            ApproximateProductWeight = x.ObstetricConsult.PesoAproxProducto.Value,
                            TA = x.ObstetricConsult.TA.Value,
                            FCM = x.ObstetricConsult.FCM.Value,
                            Edema = x.ObstetricConsult.Edema,
                            MadeUf = x.ObstetricConsult.SeHizoUf.Value,
                            Ultrasound = x.ObstetricConsult.ultrasonido,
                            PhysicalExploration = x.ObstetricConsult.exploracionFisica
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

                    consult.Diagnostics = new DiagnosticService().GetDiagnosticsByConsult(consultId);
                    consult.Treatments = new TreatmentService().GetTreatmentsByConsult(consultId);
                    consult.CabinetStudies = new StudiesService().GetCabinetStudies(consultId);
                    consult.LaboratoryStudies = new StudiesService().GetLaboratoryStudies(consultId);
                }

                return consult;
            }
        }
    }
}