using System.Threading.Tasks;
using EmailService.v1.Entities;

namespace EmailService.v1.Interfaces
{
    public interface IEmailService
    {
        Task<SendResponse> SendEmail(EmailContent content, EmailCredentials credentials);
    }
}
