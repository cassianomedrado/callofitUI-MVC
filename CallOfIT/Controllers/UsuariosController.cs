using CallOfIT.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace CallOfIT.Controllers
{
    public class UsuariosController : Controller
    {
        private static readonly string endpoint = "https://localhost:7252/api/Usuario";
        private static HttpClient httpClient = null;
        private static IEnumerable<Claim> dataUser = null;

        public UsuariosController()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(endpoint);
        }
        public async Task<IActionResult> Index()
        {
            List<Usuario> allUsuarios = new List<Usuario>();
            string dataUserJson = await GetAllUsuarios();
            if (!String.IsNullOrEmpty(dataUserJson))
            {
               allUsuarios = JsonConvert.DeserializeObject<List<Usuario>>(dataUserJson);
            }
            
            return View(allUsuarios);
        }       

        public async Task<string> GetAllUsuarios()
        {
            dataUser = User.Claims;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{TokenHolder.Token}");
            var response = await httpClient.GetAsync($"{endpoint}");

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

        public async Task<IActionResult> CadastrarUsuario(string username, string password, string confirmPassword, string email, string fullname, string userType, bool status)
        {
            var dataUsuario = new
            {
                Username = username,
                Senha = password,
                ConfirmarSenha = confirmPassword,
                Email = email,
                Fullname = fullname,
                UserType = userType,
                Status = status
            };
            string jsonData = JsonConvert.SerializeObject(dataUsuario);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenHolder.Token);
            var response = await httpClient.PostAsync($"{endpoint}/registrar", content);

            Msg msg = new Msg
            {
                Conteudo = response.Content.ToString(),
                Tipo = response.IsSuccessStatusCode ? "success" : "error"
            };

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadAsStringAsync();
                return View(reply);
            }
            else
            {
                var reply = await response.Content.ReadAsStringAsync();
                ViewBag.Msg = msg;
                return View();
            }
        }

        public IActionResult Cadastrar()
        {
            return View();
        }
        public IActionResult Alterar()
        {
            return View();
        }
    }
}
