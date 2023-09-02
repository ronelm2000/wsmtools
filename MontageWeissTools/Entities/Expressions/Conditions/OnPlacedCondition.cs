using Montage.Card.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Entities.Expressions.Conditions;
public sealed record OnPlacedCondition : WeissSchwarzCardContextCondition, IJsonDerivedTypeResolvable
{
    public static JsonDerivedType DerivedType => new JsonDerivedType(typeof(OnPlacedCondition), 4);
    
    public required WeissSchwarzZone Source { get; init; }
    public required WeissSchwarzZone Destination { get; init; }
}
