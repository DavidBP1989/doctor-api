using doctor.Models;
using doctor.Models.Consults.General;
using doctor.Models.Consults.General.Req;
using doctor.Models.Consults.General.Res;
using doctor.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace doctor.Controllers
{
    [RoutePrefix("api/treatments")]
    [Authorize(Roles = "User")]
    public class TreatmentsController : ApiController
    {
        private readonly TreatmentService service = new TreatmentService();

        [Route("{doctorId:Int}")]
        [HttpGet]
        public IEnumerable<Treatments> GetListOfTreatments(int doctorId) =>
            service.GetListOfTreatments(doctorId);


        [Route("{doctorId:Int}")]
        [HttpPost]
        public TreatmentRes SaveTreatment(int doctorId, [FromBody]TreatmentReq req) =>
            service.SaveTreatment(doctorId, req);


        [Route("{id:Int}")]
        [HttpDelete]
        public BasicResponse DeleteTreatmentById(int id) => service.DeleteTreatmentById(id);
    }
}
