using doctor.Models;
using doctor.Models.Consults;
using doctor.Models.Consults.General;
using doctor.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace doctor.Controllers
{
    [RoutePrefix("api/generalConsult")]
    [Authorize(Roles = "User")]
    public class GeneralConsultController : ApiController
    {
        readonly GeneralConsultService service = new GeneralConsultService();

        [Route("dates/{patientId:Int}")]
        [HttpGet]
        public IEnumerable<ConsultationDates> GetPreviousConsultDates(int patientId) =>
            service.GetPreviousConsultDates(patientId);


        [Route("{doctorId:Int}")]
        [HttpPost]
        public BasicResponse SaveConsult(int doctorId, [FromBody]GeneralConsult req) =>
            service.SaveConsult(doctorId, req);


        [Route("{consultId:Int}")]
        [HttpGet]
        public GeneralConsult GetConsultById(int consultId) => service.GetConsultById(consultId);
    }
}
