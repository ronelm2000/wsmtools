using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Card.API.Entities.Impls
{
    public class ActivityLog
    {
        public int LogID { get; set; }
        public ActivityType Activity { get; set; }
        public string Target { get; set; }
        public bool IsDone { get; set; } = false;
        public DateTime DateAdded { get; set; } = DateTime.Now;
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
}