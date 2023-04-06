using CallOfIT.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace CallOfIT.Controllers
{
    public class LoginController : Controller
    {
        private static readonly string endpoint = "https://localhost:7252/api/Usuario";
        private static HttpClient httpClient = null;

        public LoginController()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(endpoint);
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Json(new
                {
                    Msg = "Authenticated"
                });
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(string username, string password)
        {
            TempData.Clear();

            var reqLogin = await requestLogin(username, password);
            dynamic dataLogin = JsonConvert.DeserializeObject<object>(reqLogin.ToString());
            string token = dataLogin["token"].Value;            

            dynamic jsonDataUser = await GetUserByUsername(username, token);
            dynamic dataUserLogin = JsonConvert.DeserializeObject<object>(jsonDataUser);
            Usuario LoggedInUser = new Usuario
            {
                Id = Convert.ToInt32(dataUserLogin["id"].Value),
                Data_criacao = Convert.ToDateTime(dataUserLogin["data_criacao"].Value),
                Username = dataUserLogin["username"].Value,
                Nome = dataUserLogin["nome"].Value,
                Email = dataUserLogin["email"].Value,
                Tipo_Usuario_Id = Convert.ToInt32(dataUserLogin["tipo_usuario_id"].Value),
                Status = Convert.ToBoolean(dataUserLogin["status"].Value)
            };

            TokenHolder.Token = token;
            TokenHolder.LoggedinUser = LoggedInUser.Username;

            if (LoggedInUser.Username == username && LoggedInUser.Status == true)
            {
                List<Claim> direitosAcesso = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier,LoggedInUser.Username),
                    new Claim(ClaimTypes.Name, LoggedInUser.Nome),
                    new Claim(ClaimTypes.Authentication, token),
                    new Claim(ClaimTypes.Email, LoggedInUser.Email),
                    new Claim(ClaimTypes.Role, LoggedInUser.Tipo_Usuario_Id.ToString())
                };
                var identity = new ClaimsIdentity(direitosAcesso, "Identity.Login");
                var usuarioPrincipal = new ClaimsPrincipal(new[] { identity });

                await HttpContext.SignInAsync(usuarioPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTime.Now.AddMinutes(30)
                    });
                if (LoggedInUser.Tipo_Usuario_Id == 1)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Tecnico", "Home");
                }
                
            }
            else
            {
                TempData["MsgError"] = $"Dados de acesso incorretos ou usuário desabilitado.";
                return RedirectToAction("Index", "Login");
            }

        }
   
        public async Task<object> requestLogin(string username, string password)
        {
            var dataUsuario = new
            {
                username = username,
                senha = password
            };
            string jsonData = JsonConvert.SerializeObject(dataUsuario);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{endpoint}/login", content);
           
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

        public async Task<object> GetUserByUsername(string username, string token)
        {
            var dataUsuario = new
            {
                username = username
            };
            string jsonData = JsonConvert.SerializeObject(dataUsuario);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PostAsync($"{endpoint}", content);

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

        public async Task<IActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync();
            }
            return RedirectToAction("Index", "Login");
        }
    }
}
