using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailService.v1.Entities;
using EmailService.v1.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace EmailService.v1.Services
{
    public class EmailService : IEmailService
    {
        public async Task<SendResponse> SendEmail(EmailContent content, EmailCredentials credentials)
        {
            if (content == null || credentials == null) return SendResponse.SendFailed("Email failed to send.");

            var email = ConstructEmail(content);
            var smtpClient = await ConstructClient(credentials);

            await smtpClient.SendAsync(email);
            await smtpClient.DisconnectAsync(true);

            return SendResponse.SendSuccess("Email sent successfully.");
        }

        private static MimeMessage ConstructEmail(EmailContent content)
        {
            var email = new MimeMessage();

            email.From.AddRange(GetMailboxAddressesFromContent(content.FromNames, content.FromAddresses));
            email.To.AddRange(GetMailboxAddressesFromContent(content.ToNames, content.ToAddresses));
            email.Cc.AddRange(GetMailboxAddressesFromContent(content.CcNames, content.CcAddresses));
            email.Bcc.AddRange(GetMailboxAddressesFromContent(content.BccNames, content.BccAddresses));

            email.Subject = content.Subject;

            email.Body = new TextPart((TextFormat)content.BodyType)
                { Text = content.Body };

            return email;
        }

        private static IEnumerable<InternetAddress> GetMailboxAddressesFromContent(IEnumerable<string> names, IEnumerable<string> addresses)
        {
            if (names.Count() != addresses.Count()) return null;

            var internetAddresses = new List<InternetAddress>();

            var namesAsList = (List<string>)names;
            var addressesAsList = (List<string>)names;
            var count = namesAsList.Count();

            for (var i = 0; i < count; i++)
                internetAddresses.Add(new MailboxAddress(namesAsList[i], addressesAsList[i]));

            return internetAddresses;
        }

        private static async Task<SmtpClient> ConstructClient(EmailCredentials credentials)
        {
            var smtpClient = new SmtpClient();

            await smtpClient.ConnectAsync(credentials.Email, credentials.Port, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(credentials.Username, credentials.Password);

            return smtpClient;
        }
    }
}
