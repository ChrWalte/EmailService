namespace EmailService.v1.Entities
{
    public class SendResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }

        public SendResponse(string status, string message)
        {
            Status = status;
            Message = message;
        }

        public static SendResponse SendSuccess(string message)
            => new SendResponse("Success", message);

        public static SendResponse SendFailed(string message)
            => new SendResponse("Failed", message);
    }
}