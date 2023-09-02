using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Montage.Card.API.Entities;

namespace Montage.Weiss.Tools.Entities.Expressions.Targets;
public sealed record ThisCardTarget : WeissSchwarzTarget, IJsonDerivedTypeResolvable
{
    public static JsonDerivedType DerivedType => new JsonDerivedType(typeof(ThisCardTarget), 0);
}
