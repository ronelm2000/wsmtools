using Montage.Card.API.Entities.Impls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Card.API.Interfaces.Services
{
    public interface IActivityLogTranslator
    {
        Task<ActivityLog> Perform(ActivityLog activityLog); 
    }
}
