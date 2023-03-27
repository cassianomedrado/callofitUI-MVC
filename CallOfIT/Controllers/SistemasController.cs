using CallOfIT.Models;
using callofitAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace CallOfIT.Controllers
{
    public class SistemasController : Controller
    {
        private static readonly string endpoint = "https://localhost:7252/api/SistemaSuportado";
        private static HttpClient httpClient = null;

        public SistemasController()
        {
                httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(endpoint);
        }
        public async Task<IActionResult> Index()
        {
            List<SistemaSuportado> allSystem = new List<SistemaSuportado>();
            string dataSystemJson = await GetAllSystem();
            if (!String.IsNullOrEmpty(dataSystemJson))
            {
                allSystem = JsonConvert.DeserializeObject<List<SistemaSuportado>>(dataSystemJson);
            }
            return View(allSystem);
        }
        public async Task<string> GetAllSystem()
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

        public async Task<IActionResult> Cadastrar()
        {
            TempData.Clear();
            return View();           
        }

        public async Task<IActionResult> CadastrarSistema(IFormCollection form)
        {
            TempData.Clear();

            var dataSystem = new
            {
                nome = form["inputSistema"].FirstOrDefault(),
            };
            string jsonData = JsonConvert.SerializeObject(dataSystem);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenHolder.Token);
            var response = await httpClient.PostAsync($"{endpoint}", content);

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index", "Sistemas");
            }
            else
            {
                var reply = await response.Content.ReadAsStringAsync();

                ErrorMessage errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(reply);
                string mensagem = errorMessage.error[0].mensagem;

                TempData["MsgError"] = $"O cadastro não pode ser concluído. \n *{mensagem}.";

                return RedirectToAction("Cadastrar", "Sistemas");
            }
        }

        public async Task<IActionResult> Alterar(int id)
        {
            dynamic jsonDataSystem = await GetSystemById(id, TokenHolder.Token);
            dynamic SystemToChange = JsonConvert.DeserializeObject<object>(jsonDataSystem);
            SistemaSuportado SelectedSystem = new SistemaSuportado
            {
                id = Convert.ToInt32(SystemToChange["id"].Value),
                data_criacao = Convert.ToDateTime(SystemToChange["data_criacao"].Value),
                nome = SystemToChange["nome"].Value
            };
            return View(SelectedSystem);
        }

        public async Task<object> GetSystemById(int id, string token)
        {            
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.GetAsync($"{endpoint}/{id}");

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

        public async Task<IActionResult> AlterarSistema(IFormCollection form)
        {
            TempData.Clear();

            var dataSystem = new
            {
                id = int.Parse(form["inputSistemaID"].FirstOrDefault()),
                nome = form["inputSistema"].FirstOrDefault(),
                data_criacao = DateTime.Parse(form["inputSistemaDtCriacao"].FirstOrDefault()),
            };
            string jsonData = JsonConvert.SerializeObject(dataSystem);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenHolder.Token);
            var response = await httpClient.PutAsync($"{endpoint}", content);

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index", "Sistemas");
            }
            else
            {
                var reply = await response.Content.ReadAsStringAsync();

                ErrorMessage errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(reply);
                string mensagem = errorMessage.error[0].mensagem;

                TempData["MsgError"] = $"O cadastro não pode ser concluído. \n *{mensagem}.";

                return RedirectToAction("Cadastrar", "Sistemas");
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            dynamic jsonDataSystem = await GetSystemById(id, TokenHolder.Token);
            dynamic SystemToChange = JsonConvert.DeserializeObject<object>(jsonDataSystem);
            SistemaSuportado SelectedSystem = new SistemaSuportado
            {
                id = Convert.ToInt32(SystemToChange["id"].Value),
                data_criacao = Convert.ToDateTime(SystemToChange["data_criacao"].Value),
                nome = SystemToChange["nome"].Value
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenHolder.Token);
            var response = await httpClient.DeleteAsync($"{endpoint}/delete/{SelectedSystem.id}");

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index", "Sistemas");
            }
            else
            {
                var reply = await response.Content.ReadAsStringAsync();

                ErrorMessage errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(reply);
                string mensagem = errorMessage.error[0].mensagem;

                TempData["MsgError"] = $"O cadastro não pode ser concluído. \n *{mensagem}.";

                return RedirectToAction("Cadastrar", "Sistemas");
            }

            return View(SelectedSystem);
        }
    }
}
