using Montage.Card.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Entities.Expressions.Effects;
public record ChooseEffect : WeissSchwarzEffect, IJsonDerivedTypeResolvable
{
    public static JsonDerivedType DerivedType => new JsonDerivedType(typeof(ChooseEffect), 2);
    public required WeissSchwarzTarget Target { get; init; }
    public required StatExpression Amount { get; init; }
    public required WeissSchwarzEffect Then { get; init; }

}
