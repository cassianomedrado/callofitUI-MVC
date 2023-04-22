using CallOfIT.Models;
using CallOfIT.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CallOfIT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static readonly string endpoint = "https://localhost:7252/api/Chamado";
        private static HttpClient httpClient = null;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(endpoint);
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (TokenHolder.Tipo_Usuario_Id == 2)
                {
                    return RedirectToAction("Tecnico", "Home");
                }

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public async Task<IActionResult> Tecnico()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                if (TokenHolder.Tipo_Usuario_Id == 1)
                {
                    return RedirectToAction("Index", "Home");
                }
                
            }
           
            dynamic jsonDataUser = await GetUserByUsername(TokenHolder.LoggedinUser, TokenHolder.Token);
            dynamic dataUserLogin = JsonConvert.DeserializeObject<object>(jsonDataUser);            
            var dataUsuario = new
            {
                tecnico_usuario_id = TokenHolder.Id
            };
            //Convert.ToInt32(dataUserLogin["id"].Value)
            string jsonData = JsonConvert.SerializeObject(dataUsuario);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenHolder.Token);
            var response = await httpClient.PostAsync($"{endpoint}/chamados-consultar", content);

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadAsStringAsync();
                List<Chamado> chamados = JsonConvert.DeserializeObject<List<Chamado>>(reply);
                return View(chamados);
            }
            else
            {
                var reply = await response.Content.ReadAsStringAsync();
                return View();
            }
        }
        public async Task<object> GetUserByUsername(string username, string token)
        {
            var dataUsuario = new
            {
                username = username
            };
            string jsonData = JsonConvert.SerializeObject(dataUsuario);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PostAsync($"https://localhost:7252/api/Usuario", content);

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadAsStringAsync();
                return reply;
            }
            else
            {
                var reply = await response.Content.ReadAsStringAsync();
                return reply;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}