using System.Text.Json.Serialization;

namespace MailDraftHelper
{
    public class EmailDraftResponse
    {
        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("cvUrl")]
        public string CvUrl { get; set; }
    }
}
