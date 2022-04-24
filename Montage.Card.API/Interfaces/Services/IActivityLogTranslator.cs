using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Services;

namespace Montage.Card.API.Interfaces.Services;

/// <summary>
/// An interface that translates activity logs and executes them on the DB.
/// </summary>
public interface IActivityLogTranslator
{
    /// <summary>
    /// Performs the task as specified in the activity log.
    /// </summary>
    /// <param name="activityLog">The activity log to translate and execute.</param>
    /// <param name="progress">Allows this task to report progress as needed.</param>
    /// <param name="ct">A cancellation token that allows this task to be cancelled as needed.</param>
    /// <returns></returns>
    Task<ActivityLog> Perform(ActivityLog activityLog, IProgress<ActivityLogProgressReport> progress, CancellationToken ct = default);

    /// <summary>
    /// Performs the task as specified in the activity log.
    /// </summary>
    /// <param name="activityLog">The activity log to translate and execute.</param>
    /// <returns></returns>
    Task<ActivityLog> Perform(ActivityLog activityLog)
    {
        return Perform(activityLog, NoOpProgress<ActivityLogProgressReport>.Instance);
    }
}
