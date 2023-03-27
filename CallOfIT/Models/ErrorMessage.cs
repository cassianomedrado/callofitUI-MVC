namespace callofitAPI.Models
{
    public class ErrorMessage
    {
        public int status { get; set; }
        public List<Error> error { get; set; }
    }

    public class Error
    {
        public string mensagem { get; set; }
    }
}
