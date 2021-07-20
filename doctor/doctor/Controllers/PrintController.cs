using doctor.Models;
using doctor.Models.Print;
using doctor.Services;
using System.Web.Http;

namespace doctor.Controllers
{
    [RoutePrefix("api/print")]
    [Authorize(Roles = "User")]
    public class PrintController : ApiController
    {
        private readonly PrintService service = new PrintService();

        [Route("{doctorId:Int}")]
        [HttpGet]
        public PrintRes GetPrintConfig(int doctorId) =>
            service.GetPrintConfig(doctorId);


        [Route("{doctorId:Int}")]
        [HttpPost]
        public BasicResponse AddNewPatient(int doctorId, [FromBody] PrintReq req) =>
            service.AddNewConfig(doctorId, req);
    }
}
