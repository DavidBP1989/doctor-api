using doctor.Models.Patients;
using doctor.Models.Patients.Req;
using doctor.Models.Patients.Res;
using doctor.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using queryable = doctor.Query.QueryableAttribute;

namespace doctor.Controllers
{
    [RoutePrefix("api/patient")]
    [Authorize(Roles = "User")]
    public class PatientController : ApiController
    {
        private readonly PatientService service = new PatientService();

        [Route("{doctorId:Int}")]
        [queryable]
        [HttpGet]
        public async Task<IEnumerable<Patients>> GetListOfPatients(
            int doctorId, int? page,
            int? itemsPerPage, string columnName = null,
            string textToSearch = null, string orderby = null
            )
        {
            var result = await service.GetListOfPatients(
                doctorId,
                page ?? 1,
                itemsPerPage ?? 15,
                columnName ?? "Nombre",
                textToSearch ?? "",
                orderby ?? "name asc"
            );

            Request.Properties["x-total-count"] = result.Item1;
            return result.Item2;
        }

        [Route("assoc/{doctorId:Int}")]
        [HttpGet]
        public async Task<IEnumerable<PatientAssociation>> GetListOfPatientsByAssociation(int doctorId, string filter)
        {
            return await service.GetListOfPatientsByAssociationAsync(doctorId, filter ?? "");
        }


        [Route("byId/{patientId:Int}")]
        [HttpGet]
        public Patient GetByPatientById(int patientId) => service.GetByPatientById(patientId);


        [Route("{doctorId:Int}/last")]
        [HttpGet]
        public string GetLastEmeci(int doctorId) => service.GetLastEmeci(doctorId);


        [Route("{doctorId:Int}")]
        [HttpPost]
        public NewPatientRes AddNewPatient(int doctorId, [FromBody] NewPatientReq req) =>
            service.AddNewPatient(doctorId, req);


        [Route("newpatient")]
        [HttpGet]
        public NewPatientRes NewExistingPatient([FromUri] NewExistingPatientReq req) =>
            service.FindExistingPatient(req);
    }
}
