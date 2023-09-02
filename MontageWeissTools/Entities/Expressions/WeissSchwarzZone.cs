using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Entities.Expressions;
public abstract record WeissSchwarzZone : WeissSchwarzTarget
{
    public required Player Player { get; init; }
}

public enum Player {
    You,
    YourOpponent
}
