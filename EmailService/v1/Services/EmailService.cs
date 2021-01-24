using EmailService.v1.Entities;
using EmailService.v1.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace EmailService.v1.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailCredentials _credentials;
        private readonly CertificateInformation _information;
        private readonly bool _useCustomCertificateValidationCallback;

        public EmailService(EmailCredentials credentials, CertificateInformation information = null, bool useCustomCertificateValidationCallback = false)
        {
            _credentials = credentials;
            _information = information;
            _useCustomCertificateValidationCallback = useCustomCertificateValidationCallback;
        }

        public async Task<SendResponse> SendEmail(EmailContent content)
        {
            if (content == null || _credentials == null) return SendResponse.SendFailed("Email failed to send.");

            try
            {
                var email = ConstructEmail(content);
                var smtpClient = await ConstructClient();

                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);

                return SendResponse.SendSuccess("Email sent successfully.");
            }
            catch (Exception ex)
            {
                return SendResponse.SendFailed($"Email failed to send. {ex}");
            }
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
            var namesAsList = (List<string>)names;
            var addressesAsList = (List<string>)names;

            if (!namesAsList.Any() && !addressesAsList.Any()) return new List<InternetAddress>();
            if (namesAsList.Count != addressesAsList.Count) return null;

            var internetAddresses = new List<InternetAddress>();

            var count = namesAsList.Count;

            for (var i = 0; i < count; i++)
                internetAddresses.Add(new MailboxAddress(
                    namesAsList[i],
                    addressesAsList[i]
                    ));

            return internetAddresses;
        }

        private async Task<SmtpClient> ConstructClient()
        {
            var smtpClient = _useCustomCertificateValidationCallback
                ? new SmtpClient { ServerCertificateValidationCallback = CertificateValidationCallback }
                : new SmtpClient();

            await smtpClient.ConnectAsync(_credentials.EmailProvider, _credentials.Port, (SecureSocketOptions)_credentials.SecureSocketOptions);
            await smtpClient.AuthenticateAsync(_credentials.Username, _credentials.Password);

            return smtpClient;
        }

        private bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None) return true;

            var host = (string)sender;

            if ((errors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
                throw new Exception($"The certificate was not available for {host}.");

            if ((errors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
            {
                var certificateName = certificate is X509Certificate2 certificate2
                    ? certificate2.GetNameInfo(X509NameType.SimpleName, false)
                    : certificate.Subject;

                throw new Exception($"The Common Name for the certificate did not match {host}. Instead, it was {certificateName}.");
            }

            if (IsValidCertificate(certificate as X509Certificate2)) return true;

            var error = "The certificate for the server could not be validated for the following reasons: ";

            foreach (var element in chain.ChainElements)
            {
                error += $"\n{element.Certificate.Subject}";

                foreach (var status in element.ChainElementStatus)
                    error += $"\n\t{status.StatusInformation}";
            }

            throw new Exception(error);
        }

        private bool IsValidCertificate(X509Certificate2 certificate)
        {
            return _information?.CertificateName == certificate.GetNameInfo(X509NameType.SimpleName, false)
                   && _information?.CertificateFingerPrint == certificate.Thumbprint
                   && _information?.CertificateSerial == certificate.SerialNumber
                   && _information?.CertificateIssuer == certificate.Issuer;
        }
    }
}