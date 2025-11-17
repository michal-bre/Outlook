using System.Collections.Concurrent;
using MailDraftApi.Models;

namespace MailDraftApi.Services
{
    public static class EmailDraftStore
    {
        // token -> draft
        private static readonly ConcurrentDictionary<string, EmailDraft> _drafts
            = new ConcurrentDictionary<string, EmailDraft>();

        public static void Add(EmailDraft draft)
        {
            _drafts[draft.Token] = draft;
        }

        public static EmailDraft? GetByToken(string token)
        {
            _drafts.TryGetValue(token, out var draft);
            return draft;
        }
    }
}
