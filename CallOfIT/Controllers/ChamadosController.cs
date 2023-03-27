using Microsoft.AspNetCore.Mvc;

namespace CallOfIT.Controllers
{
    public class ChamadosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        #region Filas de Chamados

        public IActionResult Concluidos()
        {
            return View();
        }
        public IActionResult Designados()
        {
            return View();
        }
        public IActionResult Entrada()
        {
            return View();
        }
        public IActionResult Pendentes()
        {
            return View();
        }
        #endregion

        public IActionResult ChamadoUnit()
        {
            return View();
        }
    }
}
