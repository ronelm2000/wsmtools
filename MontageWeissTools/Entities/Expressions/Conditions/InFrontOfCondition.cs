using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Montage.Card.API.Entities;

namespace Montage.Weiss.Tools.Entities.Expressions.Conditions;
public sealed record InFrontOfCondition : WeissSchwarzCardContextCondition, IJsonDerivedTypeResolvable
{
    public static JsonDerivedType DerivedType => new JsonDerivedType(typeof(InFrontOfCondition), 3);

    public required WeissSchwarzTarget Target { get; init; }
}

public static class InFrontOfConditionExtensions {
    public static WeissSchwarzZone InFrontOf(this WeissSchwarzZone zone, WeissSchwarzTarget target)
        => zone with { Condition = new InFrontOfCondition { Target = target } };
}
