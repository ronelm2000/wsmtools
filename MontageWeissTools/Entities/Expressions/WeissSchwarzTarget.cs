using Montage.Weiss.Tools.Entities.Expressions.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Entities.Expressions;
public abstract record WeissSchwarzTarget
{
    public WeissSchwarzCardContextCondition? Condition { get; init; }
}

public static class Target
{
    /// <summary>
    /// Indicates a special variable that indicates "this card".
    /// </summary>
    public static WeissSchwarzTarget This { get; } = new Expressions.Targets.ThisCardTarget { };
}
