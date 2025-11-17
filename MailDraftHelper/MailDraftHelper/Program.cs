using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace MailDraftHelper
{
    public class Program
    {
        [STAThread] // חובה בשביל Outlook COM
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Helper started");

            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("No arguments provided.");
                    Console.WriteLine("Press ENTER to exit...");
                    Console.ReadLine();
                    return;
                }

                var fullUrl = args[0];
                Console.WriteLine("Full URL: " + fullUrl);

                // jobmail://create?token=xxxx  →  https://create?token=xxxx (רק בשביל ה-Uri)
                var httpUrl = fullUrl.Replace("jobmail", "https");
                var uri = new Uri(httpUrl);

                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var token = query.Get("token");

                if (string.IsNullOrWhiteSpace(token))
                {
                    Console.WriteLine("Token not provided.");
                    Console.WriteLine("Press ENTER to exit...");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine($"Token: {token}");

                await FetchDraftFromApiAndOpenOutlook(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine("FATAL ERROR in Main:");
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }

        private static async Task FetchDraftFromApiAndOpenOutlook(string token)
        {
            var apiUrl = $"https://localhost:7232/api/EmailDraft/{token}";
            Console.WriteLine($"Calling API: {apiUrl}");

            using var http = new HttpClient();

            EmailDraftResponse draft;
            try
            {
                draft = await http.GetFromJsonAsync<EmailDraftResponse>(apiUrl);
                if (draft == null)
                {
                    Console.WriteLine("Draft not found.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calling API:");
                Console.WriteLine(ex.ToString());
                return;
            }

            Console.WriteLine("Draft loaded from server:");
            Console.WriteLine($"To: {draft.To}");
            Console.WriteLine($"Subject: {draft.Subject}");
            Console.WriteLine($"Body: {draft.Body}");
            Console.WriteLine($"CV URL: {draft.CvUrl}");

            string cvLocalPath;
            try
            {
                cvLocalPath = await DownloadCvAsync(http, draft.CvUrl);
                Console.WriteLine($"CV downloaded to: {cvLocalPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading CV file:");
                Console.WriteLine(ex.ToString());
                return;
            }

            Console.WriteLine(">>> Starting to open drafts in Outlook...");

            try
            {
                await OpenMultipleOutlookDrafts(draft, cvLocalPath);
                Console.WriteLine(">>> Finished creating drafts in Outlook.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening Outlook:");
                Console.WriteLine(ex.ToString());
            }
        }

        private static async Task<string> DownloadCvAsync(HttpClient http, string cvUrl)
        {
            var uri = new Uri(cvUrl);
            var fileName = Path.GetFileName(uri.LocalPath);

            var folder = Path.Combine(Path.GetTempPath(), "MailDraftHelper");
            Directory.CreateDirectory(folder);

            var localPath = Path.Combine(folder, fileName);

            var bytes = await http.GetByteArrayAsync(cvUrl);
            await File.WriteAllBytesAsync(localPath, bytes);

            return localPath;
        }

        private static async Task OpenMultipleOutlookDrafts(EmailDraftResponse draft, string cvPath)
        {
            Console.WriteLine("Inside OpenMultipleOutlookDrafts");

            Outlook.Application outlook;
            try
            {
                outlook = new Outlook.Application();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create Outlook.Application:");
                Console.WriteLine(ex.ToString());
                return;
            }

            var recipients = (draft.To ?? "")
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList();

            Console.WriteLine($"Recipients count: {recipients.Count}");

            if (!recipients.Any())
            {
                Console.WriteLine("No recipients found.");
                return;
            }

            foreach (var recipient in recipients)
            {
                try
                {
                    Console.WriteLine($"Creating draft for: {recipient}");

                    var mail = (Outlook.MailItem)outlook.CreateItem(Outlook.OlItemType.olMailItem);

                    mail.To = recipient;
                    mail.Subject = draft.Subject;
                    mail.Body = draft.Body;

                    if (File.Exists(cvPath))
                    {
                        mail.Attachments.Add(cvPath);
                    }

                    mail.Save();
                    Console.WriteLine($"Draft saved for {recipient}");

                    try
                    {
                        mail.Display(false);
                        Console.WriteLine($"Displayed draft for {recipient}");
                    }
                    catch (Exception exDisplay)
                    {
                        Console.WriteLine($"Failed to display draft for {recipient}: {exDisplay.Message}");
                    }

                    await Task.Delay(350);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating draft for {recipient}:");
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
