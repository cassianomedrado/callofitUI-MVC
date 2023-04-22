using CallOfIT.Models;
using callofitAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace CallOfIT.Controllers
{
    public class UsuariosController : Controller
    {
        private static readonly string endpoint = "https://localhost:7252/api/Usuario";
        private static HttpClient httpClient = null;

        public UsuariosController()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(endpoint);
        }
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (TokenHolder.Tipo_Usuario_Id == 2)
                {
                    return RedirectToAction("Tecnico", "Home");
                }

                List<Usuario> allUsuarios = new List<Usuario>();
                string dataUserJson = await GetAllUsuarios();
                if (!String.IsNullOrEmpty(dataUserJson))
                {
                    allUsuarios = JsonConvert.DeserializeObject<List<Usuario>>(dataUserJson);
                }

                return View(allUsuarios);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }  
        }

        public async Task<string> GetAllUsuarios()
        {
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

        public async Task<IActionResult> CadastrarUsuario(IFormCollection form)
        {
            TempData.Clear();

            var dataUsuario = new
            {
                username = form["username"].FirstOrDefault(),
                senha = form["password"].FirstOrDefault(),
                confirmaSenha = form["confirmPassword"].FirstOrDefault(),
                email = form["email"].FirstOrDefault(),
                nome = form["fullname"].FirstOrDefault(),
                tipo_usuario_id = form["userType"].FirstOrDefault(),
                status = form["status"].Equals("on") ? true : false
            };

            if (String.IsNullOrEmpty(dataUsuario.username))
            {
                TempData["MsgError"] = $"O nome usuário é obrigatório.";
            }

            if (String.IsNullOrEmpty(dataUsuario.senha))
            {
                TempData["MsgError"] = $"A senha é obrigatório.";
            }

            if (String.IsNullOrEmpty(dataUsuario.confirmaSenha))
            {
                TempData["MsgError"] = $"A confirmação da senha é obrigatório.";
            }

            if (String.IsNullOrEmpty(dataUsuario.email))
            {
                TempData["MsgError"] = $"O email é obrigatório.";
            }

            if (String.IsNullOrEmpty(dataUsuario.nome))
            {
                TempData["MsgError"] = $"O nome é obrigatório.";
            }
            if (String.IsNullOrEmpty(dataUsuario.username) || String.IsNullOrEmpty(dataUsuario.senha) ||
                String.IsNullOrEmpty(dataUsuario.confirmaSenha) || String.IsNullOrEmpty(dataUsuario.email) ||
                String.IsNullOrEmpty(dataUsuario.nome))
            {

                return RedirectToAction("Cadastrar", "Usuarios");
            }

            string jsonData = JsonConvert.SerializeObject(dataUsuario);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenHolder.Token);
            var response = await httpClient.PostAsync($"{endpoint}/registrar", content);

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index", "Usuarios");
            }
            else
            {
                var reply = await response.Content.ReadAsStringAsync();

                ErrorMessage errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(reply);
                string mensagem = errorMessage.error[0].mensagem;

                TempData["MsgError"] = $"O cadastro não pode ser concluído. \n *{mensagem}.";

                return RedirectToAction("Cadastrar", "Usuarios");
            }
        }

        public async Task<IActionResult> Cadastrar()
        {
            List<TipoUsuario> tipoUsuario = await GetAllTipoUsuario();
            ViewBag.TipoUsuario = tipoUsuario;
            return View();
        }

        public async Task<IActionResult> Alterar(string username)
        {
            dynamic jsonDataUser = await GetUserByUsername(username, TokenHolder.Token);
            dynamic UserToChange = JsonConvert.DeserializeObject<object>(jsonDataUser);
            Usuario SelectedUser = new Usuario
            {
                Id = Convert.ToInt32(UserToChange["id"].Value),
                Data_criacao = Convert.ToDateTime(UserToChange["data_criacao"].Value),
                Username = UserToChange["username"].Value,
                Nome = UserToChange["nome"].Value,
                Email = UserToChange["email"].Value,
                Tipo_Usuario_Id = Convert.ToInt32(UserToChange["tipo_usuario_id"].Value),
                Status = Convert.ToBoolean(UserToChange["status"].Value)
            };

            List<TipoUsuario> tipoUsuario = await GetAllTipoUsuario();
            ViewBag.TipoUsuario = tipoUsuario;

            return View(SelectedUser);
        }

        public async Task<IActionResult> AlterarCadastro(IFormCollection form)
        {
            TempData.Clear();

            var dataUsuario = new
            {
                id = form["id"].FirstOrDefault(),
                username = form["username"].FirstOrDefault(),
                email = form["email"].FirstOrDefault(),
                nome = form["nome"].FirstOrDefault(),
                tipo_usuario_id = form["userType"].FirstOrDefault(),
                status = form["status"].Equals("on") ? true : false
            };

            string jsonData = JsonConvert.SerializeObject(dataUsuario);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenHolder.Token);
            var response = await httpClient.PutAsync($"{endpoint}/update", content);

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index", "Usuarios");
            }
            else
            {
                var reply = await response.Content.ReadAsStringAsync();

                ErrorMessage errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(reply);
                string mensagem = errorMessage.error[0].mensagem;

                TempData["MsgError"] = $"O cadastro não pode ser concluído. \n *{mensagem}.";

                return RedirectToAction("Alterar", "Usuarios");
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

        public async Task<IActionResult> AlterarStatus(string username)
        {
            TempData.Clear();

            var userSelected = await GetUserByUsername(username, TokenHolder.Token);

            if (userSelected != null)
            {
                dynamic dataUserLogin = JsonConvert.DeserializeObject<object>(userSelected.ToString());

                Usuario userFound = new Usuario
                {
                    Id = Convert.ToInt32(dataUserLogin["id"].Value),
                    Data_criacao = Convert.ToDateTime(dataUserLogin["data_criacao"].Value),
                    Username = dataUserLogin["username"].Value,
                    Nome = dataUserLogin["nome"].Value,
                    Email = dataUserLogin["email"].Value,
                    Tipo_Usuario_Id = Convert.ToInt32(dataUserLogin["tipo_usuario_id"].Value),
                    Status = Convert.ToBoolean(dataUserLogin["status"].Value)
                };

                //Se Usuário Estiver Inativo passa a ser ativado
                if (!userFound.Status)
                {
                    userFound.Status = true;
                }                
                else
                {
                    userFound.Status = false;
                }

                string jsonData = JsonConvert.SerializeObject(userFound);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenHolder.Token);
                var response = await httpClient.PostAsync($"{endpoint}/alterar-status", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Usuarios");
                }
                else
                {
                    var reply = await response.Content.ReadAsStringAsync();

                    ErrorMessage errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(reply);
                    string mensagem = errorMessage.error[0].mensagem;

                    TempData["MsgError"] = $"A inativação do usuário não pode ser concluída. \n *{mensagem}.";

                    return RedirectToAction("Index", "Usuarios");
                }

            }
            else
            {
                TempData["MsgError"] = $"A inativação do usuário não pode ser concluída. O usuário selecionado não foi localizado para concluir a operação.";

                return RedirectToAction("Index", "Usuarios");
            }
        }



        public async Task<List<TipoUsuario>> GetAllTipoUsuario()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{TokenHolder.Token}");
            var response = await httpClient.GetAsync($"https://localhost:7252/api/TipoUsuario");

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadAsStringAsync();
                List<TipoUsuario> objTipoUsuario = JsonConvert.DeserializeObject<List<TipoUsuario>>(reply);
                return objTipoUsuario;
            }
            else
            {
                return null;
            }
        }

    }
}
