using System.Diagnostics.CodeAnalysis;

namespace Montage.Card.API.Entities.Impls;

public class ActivityLog
{
    public static IEqualityComparer<ActivityLog> EqualById { get; } = new ActivityLogIDEqualityComparer();

    public int LogID { get; set; }
    public ActivityType Activity { get; set; }
    public string? Target { get; set; }
    public bool IsDone { get; set; } = false;
    public DateTime DateAdded { get; set; } = DateTime.Now;

    private class ActivityLogIDEqualityComparer : IEqualityComparer<ActivityLog>
    {
        public bool Equals(ActivityLog? x, ActivityLog? y) => x?.LogID == y?.LogID;
        public int GetHashCode([DisallowNull] ActivityLog obj) => obj.LogID.GetHashCode();
    }
}

public enum ActivityType
{
    Parse = 0,
    Delete = 1
}

public static class ActivityExtensions
{
    public static string ToVerbString(this ActivityType actType)
    {
        return actType switch
        {
            ActivityType.Parse => "Parsing",
            ActivityType.Delete => "Deleting",  
            _ => throw new NotImplementedException()
        };
    }
}
