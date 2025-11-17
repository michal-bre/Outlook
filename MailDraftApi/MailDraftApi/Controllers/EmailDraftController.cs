using MailDraftApi.Dtos;
using MailDraftApi.Models;
using MailDraftApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace MailDraftApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailDraftController : ControllerBase
    {
        private readonly UploadSettings _uploadSettings;
        private readonly IWebHostEnvironment _env;

        public EmailDraftController(UploadSettings uploadSettings, IWebHostEnvironment env)
        {
            _uploadSettings = uploadSettings;
            _env = env;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateDraft(
    [FromForm] CreateEmailDraftRequest model)
        {
            var cv = model.Cv;

            if (cv == null || cv.Length == 0)
            {
                return BadRequest("יש לצרף קובץ קורות חיים");
            }

            // 1. שמירת הקובץ
            var ext = Path.GetExtension(cv.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var cvFolder = _uploadSettings.CvFolder;
            var filePath = Path.Combine(cvFolder, fileName);

            Directory.CreateDirectory(cvFolder);

            using (var stream = System.IO.File.Create(filePath))
            {
                await cv.CopyToAsync(stream);
            }

            // 2. יצירת token
            var token = Guid.NewGuid().ToString("N");
            var cvUrl = $"/uploads/Cv/{fileName}";

            var draft = new EmailDraft
            {
                Token = token,
                To = model.To,
                Subject = model.Subject,
                Body = model.Body,
                CvPath = filePath,
                CvUrl = cvUrl,
                CreatedAt = DateTime.UtcNow
            };

            EmailDraftStore.Add(draft);

            return Ok(new { token });
        }



        [HttpGet("{token}")]
        public IActionResult GetDraft(string token)
        {
            var draft = EmailDraftStore.GetByToken(token);
            if (draft == null)
                return NotFound();

            // מה שהאפליקציה המקומית תשתמש
            return Ok(new
            {
                to = draft.To,
                subject = draft.Subject,
                body = draft.Body,
                cvUrl = $"{Request.Scheme}://{Request.Host}{draft.CvUrl}"
            });
        }
    }
}
