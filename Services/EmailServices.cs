using API.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace API.Services
{
    public class EmailServices : IEmailServices
    {
        private readonly ISendGridClient _client;
        public EmailServices(ISendGridClient client)
        {
            _client = client;
        }

        public async void sendEmail(string email, string code){
            var from = new EmailAddress("tcc.travel.app@gmail.com", "Travel");
            var subject = "Seu código de validação";
            var to = new EmailAddress(email, $"{code}");
            var plainTextContent = $"Seu código de validação é {code}";
            var htmlContent = $"<strong>Seu código de validação é {code}</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await _client.SendEmailAsync(msg).ConfigureAwait(false);
        }

        public async void sendNewPassword(string email, string password){
            var from = new EmailAddress("tcc.travel.app@gmail.com", "Travel");
            var subject = "Sua nova senha";
            var to = new EmailAddress(email, password);
            var plainTextContent = $"Sua nova senha é {password}";
            var htmlContent = $"<strong>Sua nova senha é {password}</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await _client.SendEmailAsync(msg).ConfigureAwait(false);
        }
    }
}