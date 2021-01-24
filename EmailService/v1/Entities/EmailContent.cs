using EmailService.v1.Enums;
using System.Collections.Generic;

namespace EmailService.v1.Entities
{
    public class EmailContent
    {
        public List<string> FromNames { get; set; }
        public List<string> FromAddresses { get; set; }
        public List<string> ToNames { get; set; }
        public List<string> ToAddresses { get; set; }
        public List<string> CcNames { get; set; }
        public List<string> CcAddresses { get; set; }
        public List<string> BccNames { get; set; }
        public List<string> BccAddresses { get; set; }
        public string Subject { get; set; }
        public TextFormat BodyType { get; set; }
        public string Body { get; set; }
    }
}