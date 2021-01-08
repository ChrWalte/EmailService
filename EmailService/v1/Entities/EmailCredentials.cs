using EmailService.v1.Enums;

namespace EmailService.v1.Entities
{
    public class EmailCredentials
    {
        public string EmailProvider { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public SecureSocketOptions SecureSocketOptions { get; set; }
    }
}
