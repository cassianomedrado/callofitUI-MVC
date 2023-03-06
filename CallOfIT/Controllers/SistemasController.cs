using Microsoft.AspNetCore.Mvc;

namespace CallOfIT.Controllers
{
    public class SistemasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
