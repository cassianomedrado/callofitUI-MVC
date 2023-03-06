namespace CallOfIT.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public DateTime Data_criacao { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public int Tipo_Usuario_Id { get; set; }
        public string Username { get; set; }
        public string Senha { get; set; }
        public Boolean Status { get; set; }
    }
}
