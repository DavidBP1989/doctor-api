using doctor.Models;
using doctor.Models.Consults;
using doctor.Models.Consults.Obstetric;
using doctor.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace doctor.Controllers
{
    [RoutePrefix("api/obstetricConsult")]
    [Authorize(Roles = "User")]
    public class ObstetricConsultController : ApiController
    {
        readonly ObstetricConsultService service = new ObstetricConsultService();

        [HttpGet]
        [Route("dates/{patientId:Int}")]
        public IEnumerable<ConsultationDates> GetPreviousConsultDates(int patientId) =>
            service.GetPreviousConsultDates(patientId);


        [HttpPost]
        [Route("{doctorId:Int}")]
        public BasicResponse SaveConsult(int doctorId, [FromBody]ObstetricConsult req) =>
            service.SaveConsult(doctorId, req);


        [HttpGet]
        [Route("{consultId:Int}")]
        public ObstetricConsult GetConsultById(int consultId) => service.GetConsultById(consultId);
    }
}
