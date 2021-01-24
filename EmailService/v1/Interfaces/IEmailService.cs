using EmailService.v1.Entities;
using System.Threading.Tasks;

namespace EmailService.v1.Interfaces
{
    public interface IEmailService
    {
        Task<SendResponse> SendEmail(EmailContent content);
    }
}