namespace CallOfIT.Models
{
    public class Chamado
    {
        public int id { get; set; }
        public DateTime data_criacao { get; set; }
        public string solicitante { get; set; }
        public DateTime data_limite { get; set; }
        public int status_chamado_id { get; set; }
        public int sistema_suportado_id { get; set; }
        public string descricao_problema { get; set; }
        public int usuario_id { get; set; }
        public int? tecnico_usuario_id { get; set; }
        public string descricao_solucao { get; set; }
        public int tipo_chamado_id { get; set; }
        public string identificador_unico { get; set; }

    }
}
