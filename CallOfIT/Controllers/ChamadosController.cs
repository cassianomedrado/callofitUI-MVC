using CallOfIT.Models;
using callofitAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace CallOfIT.Controllers
{
    public class ChamadosController : Controller
    {
        private static readonly string endpoint = "https://localhost:7252/api/Chamado";
        private static HttpClient httpClient = null;

        public ChamadosController()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(endpoint);
        }
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if(TokenHolder.Tipo_Usuario_Id == 2)
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

        #region Filas de Chamados

        public async Task<IActionResult> Concluidos()
        {
            var chamados = await GetChamadosStatus(3);
            List<Chamado> chamadosFinalizados = JsonConvert.DeserializeObject<List<Chamado>>(chamados);
            if (chamadosFinalizados != null)
            {
                return View(chamadosFinalizados);
            }
            else
            {
                TempData["MsgError"] = $"Não há registros.";
                return RedirectToAction("Index", "Chamados");
            }
           
        }
        public async Task<IActionResult> Designados()
        {
            TempData.Clear();
            var chamados = await GetAllChamados();
            List<Chamado> chamadosDesignados = JsonConvert.DeserializeObject<List<Chamado>>(chamados);
            var chamadosfiltro = chamadosDesignados.Where(cd => cd.tecnico_usuario_id != null).ToList();
            if (chamadosfiltro != null)
            {
                return View(chamadosfiltro);
            }
            else
            {
                TempData["MsgError"] = $"Não há registros.";
                return RedirectToAction("Index", "Chamados");
            }
            
        }
        public async Task<IActionResult> Entrada()
        {
            TempData.Clear();
            var chamados = await GetAllChamados();
            List<Chamado> chamadosNaoDesignados = JsonConvert.DeserializeObject<List<Chamado>>(chamados);
            var chamadofiltro = chamadosNaoDesignados.Where(cnd => cnd.tecnico_usuario_id == null && cnd.status_chamado_id == 1).ToList();
            if (chamadofiltro != null)
            {
                return View(chamadofiltro);
            }
            else
            {
                TempData["MsgError"] = $"Não há registros.";
                return RedirectToAction("Index", "Chamados");
            }            
        }
        public async Task<IActionResult> Pendentes()
        {
            var chamados = await GetChamadosStatus(4);
            List<Chamado> chamadosAtrasados = JsonConvert.DeserializeObject<List<Chamado>>(chamados);
            if (chamadosAtrasados != null)
            {
                return View(chamadosAtrasados);
            }
            else
            {
                TempData["MsgError"] = $"Não há registros.";
                return RedirectToAction("Index", "Chamados");
            }
        }
        public async Task<IActionResult> ChamadoUnit(int id)
        {
            TempData.Clear();
            var chamado = await GetChamadosByID(id);
            Chamado objChamado = JsonConvert.DeserializeObject<Chamado>(chamado);

            if (objChamado != null)
            {
                #region Status Chamado
                var statusChamados = await GetStatusChamdoByID(objChamado.status_chamado_id);
                StatusChamados objStatusChamados = JsonConvert.DeserializeObject<StatusChamados>(statusChamados);
                TempData["statusChamados"] = $"{objStatusChamados.descricao}";
                #endregion

                #region Tipo Chamado
                var tiposChamado = await GetTipoChamadoByID();
                List<TipoChamado> allTiposChamado = JsonConvert.DeserializeObject<List<TipoChamado>>(tiposChamado);
                ViewBag.TiposChamado = allTiposChamado;
                #endregion

                #region Solicitante
                Usuario solicitante = await GetUsuarioByID(objChamado.usuario_id);
                ViewBag.Solicitante = solicitante;
                #endregion

                #region Status Chamado
                var allStatusChamados = await GetAllStatusChamados();
                ViewBag.StatusChamados = allStatusChamados;
                #endregion

                #region Tecnico Designado
                var allTecnico = await GetAllTecnico();
                ViewBag.Tecnicos = allTecnico;
                #endregion

                #region Sistema Suportado
                var objSistemasSuportados = await GetSistemasSuportado();
                List<SistemaSuportado> sistemasSuportados = JsonConvert.DeserializeObject<List<SistemaSuportado>>(objSistemasSuportados);
                ViewBag.SistemasSuportados = sistemasSuportados;
                #endregion

                return View(objChamado);
            }
            else
            {
                TempData["MsgError"] = $"Chamado não localizado.";
                return RedirectToAction("Index", "Chamados");
            }
        }
        #endregion

        #region Funções
        public async Task<string> GetAllChamados()
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
        public async Task<string> GetChamadosStatus(int idStatusChamado)
        {
            var dataChamados = new
            {
                status_chamado_id = idStatusChamado
            };
            string jsonData = JsonConvert.SerializeObject(dataChamados);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{TokenHolder.Token}");
            var response = await httpClient.PostAsync($"{endpoint}/chamados-consultar", content);

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
        public async Task<string> GetChamadosByID(int id)
        {            
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{TokenHolder.Token}");
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
        public async Task<string> GetStatusChamdoByID(int id)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{TokenHolder.Token}");
            var response = await httpClient.GetAsync($"https://localhost:7252/api/StatusChamado/{id}");

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
        public async Task<Usuario> GetUsuarioByID(int? id)
        {  
            var dataChamados = new
            {
                id = id
            };
            string jsonData = JsonConvert.SerializeObject(dataChamados);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{TokenHolder.Token}");
            var response = await httpClient.PostAsync($"https://localhost:7252/api/Usuario/Usuario-por-id/", content);

            Usuario usuario = new Usuario();

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadAsStringAsync();
                usuario = JsonConvert.DeserializeObject<Usuario>(reply);                
            }
            return usuario;
        }
        public async Task<string> GetTipoChamadoByID()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{TokenHolder.Token}");
            var response = await httpClient.GetAsync($"https://localhost:7252/api/TipoChamado");

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
        public async Task<string> GetSistemasSuportado()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{TokenHolder.Token}");
            var response = await httpClient.GetAsync($"https://localhost:7252/api/SistemaSuportado");

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

        public async Task<List<StatusChamados>> GetAllStatusChamados()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{TokenHolder.Token}");
            var response = await httpClient.GetAsync($"https://localhost:7252/api/StatusChamado");

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadAsStringAsync();
                List<StatusChamados> objStatusChamados = JsonConvert.DeserializeObject<List<StatusChamados>>(reply);
                return objStatusChamados;
            }
            else
            {
                var reply = await response.Content.ReadAsStringAsync();
                return null;
            }
        }

        public async Task<List<Usuario>> GetAllTecnico()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{TokenHolder.Token}");
            var response = await httpClient.GetAsync($"https://localhost:7252/api/Usuario");

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadAsStringAsync();
                List<Usuario> objUsers = JsonConvert.DeserializeObject<List<Usuario>>(reply);
                var technicians = objUsers.Where(tec => tec.Tipo_Usuario_Id == 2).ToList();
                return technicians;
            }
            else
            {
                var reply = await response.Content.ReadAsStringAsync();
                return null;
            }
        }

        public async Task<List<Usuario>> GetAllUsuario()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{TokenHolder.Token}");
            var response = await httpClient.GetAsync($"https://localhost:7252/api/Usuario");

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadAsStringAsync();
                List<Usuario> objUsers = JsonConvert.DeserializeObject<List<Usuario>>(reply);
                return objUsers;
            }
            else
            {
                var reply = await response.Content.ReadAsStringAsync();
                return null;
            }
        }

        public async Task<IActionResult> AlterarChamado(IFormCollection form)
        {
            //TempData.Clear();

            var dataChamado = new
            {
                id = form["inputID"].FirstOrDefault(),
                sistema_suportado_id = form["inputSistemasSuportados"].FirstOrDefault(),
                data_criacao = DateTime.Parse(form["inputDtCriacao"].FirstOrDefault()),
                data_limite = DateTime.Parse(form["inputDtLimite"].FirstOrDefault()),
                solicitante = form["inputSolicitante"].FirstOrDefault(),
                tipo_chamado_id = form["inputTipoChamado"].FirstOrDefault(), 
                status_chamado_id = form["inputStatusChamado"].FirstOrDefault(),
                tecnico_usuario_id = form["inputTecnicoDesignado"].FirstOrDefault() == "null" ? null : form["inputTecnicoDesignado"].FirstOrDefault(),
                descricao_problema = form["inputDescricaoProblema"].FirstOrDefault(), 
                descricao_solucao = form["inputDescricaoSolucao"].FirstOrDefault(), 
            };

            var teste = form["inputTecnicoDesignado"].FirstOrDefault();

            string jsonData = JsonConvert.SerializeObject(dataChamado);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenHolder.Token);
            var response = await httpClient.PutAsync($"{endpoint}", content);

            if (response.IsSuccessStatusCode)
            {
                var reply = await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index", "Chamados");
            }
            else
            {
                var reply = await response.Content.ReadAsStringAsync();

                ErrorMessage errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(reply);
                string mensagem = errorMessage.error[0].mensagem;

                TempData["MsgError"] = $"Ocorreu um erro. \n *{mensagem}.";

                return RedirectToAction("Index", "Chamados");
            }
        }
        #endregion
    }
}
