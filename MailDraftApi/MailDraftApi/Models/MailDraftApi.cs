namespace MailDraftApi.Models
{
    public class EmailDraft
    {
        public int Id { get; set; }         
        public string Token { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string CvPath { get; set; } = string.Empty;   // נתיב פיזי לקובץ
        public string CvUrl { get; set; } = string.Empty;    // כתובת להורדה מהשרת
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
