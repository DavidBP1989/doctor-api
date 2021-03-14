using doctor.Models.Consults.General;
using doctor.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace doctor.Controllers
{
    [RoutePrefix("api/cabinet")]
    [Authorize(Roles = "User")]
    public class CabinetStudiesController : ApiController
    {
        private readonly StudiesService service = new StudiesService();

        [Route("")]
        [HttpGet]
        public IEnumerable<CategoryStudies> GetStudies() =>
            service.GetStudiesByType(StudiesService.TypeCategory.Cabinet);
    }
}
