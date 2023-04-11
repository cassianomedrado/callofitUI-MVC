using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CallOfIT.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/NotFound")]
        public IActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View("~/Views/Shared/NotFound.cshtml");
        }
    }
}
