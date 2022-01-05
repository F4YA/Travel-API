namespace API.Interfaces
{
    public interface IEmailServices
    {
        public void sendEmail(string email, string code);
        public void sendNewPassword(string email, string password);
    }
}