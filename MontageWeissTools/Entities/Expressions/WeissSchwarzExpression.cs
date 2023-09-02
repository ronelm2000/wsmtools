using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Entities.Expressions;

public record WeissSchwarzExpression 
{
    public required ExpressionType ExpType { get; init; }
    public required Label[] Labels { get; init; }
    public WeissSchwarzCondition? Condition { get; init; }
    public required WeissSchwarzEffect Effect { get; init; }
}

public enum ExpressionType
{
    Automated,
    Continuous,
    Activated,
    Replay
}

public enum Label
{
    Assist,
    Bodyguard,
    Memory,
    Experience,
    Brainstorm
}
