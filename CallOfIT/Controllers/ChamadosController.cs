using Microsoft.AspNetCore.Mvc;

namespace CallOfIT.Controllers
{
    public class ChamadosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
