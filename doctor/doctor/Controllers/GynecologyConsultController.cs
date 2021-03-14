using doctor.Models;
using doctor.Models.Consults;
using doctor.Models.Consults.Gynecology;
using doctor.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace doctor.Controllers
{
    [RoutePrefix("api/gynecologyConsult")]
    [Authorize(Roles = "User")]
    public class GynecologyConsultController : ApiController
    {
        readonly GynecologyConsultService service = new GynecologyConsultService();

        [HttpGet]
        [Route("dates/{patientId:Int}")]
        public IEnumerable<ConsultationDates> GetPreviousConsultDates(int patientId) =>
            service.GetPreviousConsultDates(patientId);


        [HttpPost]
        [Route("{doctorId:Int}")]
        public BasicResponse SaveConsult(int doctorId, [FromBody]GynecologyConsult req) =>
            service.SaveConsult(doctorId, req);


        [HttpGet]
        [Route("{consultId:Int}")]
        public GynecologyConsult GetConsultById(int consultId) => service.GetConsultById(consultId);
    }
}
