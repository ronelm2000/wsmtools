using Lamar;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Services
{
    public class ActivityLogTranslator : IActivityLogTranslator
    {
        private Func<CardDatabaseContext> _db;

        public ActivityLogTranslator(IContainer ioc)
        {
            _db = () => ioc.GetInstance<CardDatabaseContext>();
        }

        public async Task<ActivityLog> Perform(ActivityLog activityLog)
        {
            using (var db = _db())
            {
                Func<Task> action = activityLog.Activity switch
                {
                    ActivityType.Delete => async () => await DeleteCards(db, activityLog),
                    _ => throw new NotImplementedException($"{activityLog.Activity} has not been implemented yet!")
                };
                try
                {
                    await action();
                    activityLog.IsDone = true;
                    return activityLog;
                }
                catch (Exception e)
                {
                    throw e;
                    // return false;
                }
            }
        }

        private async Task DeleteCards(CardDatabaseContext db, ActivityLog activityLog)
        {
            var deleteArgs = JsonSerializer.Deserialize<DeleteArgs>(activityLog.Target);
            var query = db.WeissSchwarzCards.AsQueryable();
            if (!string.IsNullOrWhiteSpace(deleteArgs.Language))
            {
                var lang = TranslateLanguage(deleteArgs.Language);
                query = query.Where(card => card.Language == lang);
            }
            if (!string.IsNullOrWhiteSpace(deleteArgs.Language))
            {
                var version = new Version(deleteArgs.VersionLessThan);
                query = query.Where(card => new Version(card.VersionTimestamp) < version);
            }
            db.RemoveRange(query);
            await db.SaveChangesAsync();
        }

        private CardLanguage TranslateLanguage(string language) => language.ToLower() switch
         {
             "en" => CardLanguage.English,
             "jp" => CardLanguage.Japanese,
             _ => throw new ActivityLogExecutionException($"Cannot translate {language} into CardLanguage")
         };
    }

    struct DeleteArgs
    {
        public string Language;
        public string VersionLessThan;
    }
}
