using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Montage.Card.API.Entities;

namespace Montage.Weiss.Tools.Entities.Expressions.Zones;
public sealed record Stage : WeissSchwarzZone, IJsonDerivedTypeResolvable
{
    public static JsonDerivedType DerivedType => new JsonDerivedType(typeof(Stage), 1);
}

public static class StageExtensions
{
    public static Stage Stage(this Player player)
        => new Stage { Player = player };
}