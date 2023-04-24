using CallOfIT.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using callofitAPI.Models;

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
                if (TokenHolder.Tipo_Usuario_Id == 2)
                {
                    return RedirectToAction("Tecnico", "Home");
                }

                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(string username, string password)
        {
            TempData.Clear();

            var response = await requestLogin(username, password);
            var reply = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ErrorMessage>(reply);
                var i = 0;
                if(result.error != null)
                {
                    foreach (var er in result.error)
                    {
                        i++;
                        if (i == result.error.Count)
                        {
                            TempData["MsgError"] += er.mensagem;
                        }
                        else
                        {
                            TempData["MsgError"] += er.mensagem + "\n";
                        }
                    }
                }
    
                return RedirectToAction("Index", "Login");
            }

            dynamic dataLogin = JsonConvert.DeserializeObject<object>(reply.ToString());

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

            if(LoggedInUser.Tipo_Usuario_Id == 3)
            {
                if (User.Identity.IsAuthenticated)
                {
                    await HttpContext.SignOutAsync();
                }
                TempData["MsgError"] = "Usuário não tem autorização para acessar o sistema administrativo.";
                return RedirectToAction("Index", "Login");
            }

            TokenHolder.Token = token;
            TokenHolder.LoggedinUser = LoggedInUser.Username;
            TokenHolder.Tipo_Usuario_Id = LoggedInUser.Tipo_Usuario_Id;
            TokenHolder.Id = LoggedInUser.Id;

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
   
        public async Task<HttpResponseMessage> requestLogin(string username, string password)
        {
            var dataUsuario = new
            {
                username = username,
                senha = password
            };
            string jsonData = JsonConvert.SerializeObject(dataUsuario);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            return await httpClient.PostAsync($"{endpoint}/login", content);
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
