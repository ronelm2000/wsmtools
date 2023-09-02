using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Montage.Card.API.Entities;

namespace Montage.Weiss.Tools.Entities.Expressions.Conditions;
public sealed record AndCondition : WeissSchwarzCardContextCondition, IJsonDerivedTypeResolvable
{
    public static JsonDerivedType DerivedType => new JsonDerivedType(typeof(AndCondition), 1);

    public required WeissSchwarzCondition[] Conditions { get; init; }
}
