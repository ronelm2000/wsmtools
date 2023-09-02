using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Montage.Card.API.Entities;

namespace Montage.Weiss.Tools.Entities.Expressions.Effects;


public sealed record GainStatEffect : WeissSchwarzEffect, IJsonDerivedTypeResolvable
{
    public static JsonDerivedType DerivedType => new JsonDerivedType(typeof(GainStatEffect), 1);

    public required WeissSchwarzTarget Target { get; init; }
    public required StatType Stat { get; init; }
    public required StatExpression Expression { get; init; } 
}

public enum StatType
{
    ATK,
    Soul,
    Level,
    Cost
}

[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
[JsonDerivedType(typeof(ConstantStatExpression), 1)]
[JsonDerivedType(typeof(LevelCharacterStatExpression), 2)]
public abstract record StatExpression
{
}

/// <summary>
/// Indicates a constant stat level expression.
/// </summary>
public record ConstantStatExpression : StatExpression
{
    public required int Value { get; init; }
}

/// <summary>
/// Indicates an expression X. X is equal to that character's level mutiplied by [value].
/// </summary>
public record LevelCharacterStatExpression : StatExpression
{
    public required int Value { get; init; }
}

/// <summary>
/// Indicates an expression X. X is equal to [player]'s level mutiplied by [value].
/// </summary>
public record LevelPlayerStatExpression : StatExpression
{
    public required Player Player { get; init; }
    public required int Value { get; init; }
}