using doctor.Models;
using doctor.Models.Consults.General;
using doctor.Models.Consults.General.Req;
using doctor.Models.Consults.General.Res;
using doctor.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace doctor.Controllers
{
    [RoutePrefix("api/diagnostics")]
    [Authorize(Roles = "User")]
    public class DiagnosticsController : ApiController
    {
        private readonly DiagnosticService service = new DiagnosticService();

        [Route("{doctorId:Int}")]
        [HttpGet]
        public IEnumerable<Diagnostics> GetListOfDiagnostics(int doctorId) =>
            service.GetListOfDiagnostics(doctorId);


        [Route("{doctorId:Int}")]
        [HttpPost]
        public DiagnosticRes SaveDiagnostic(int doctorId, [FromBody]DiagnosticReq req) =>
            service.SaveDiagnostic(doctorId, req);


        [Route("{id:Int}")]
        [HttpDelete]
        public BasicResponse DeleteDiagnosticById(int id) => service.DeleteDiagnosticById(id);
    }
}
