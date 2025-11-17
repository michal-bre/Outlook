namespace MailDraftApi.Dtos
{
    public class CreateEmailDraftRequest
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public IFormFile Cv { get; set; }

    }
}
