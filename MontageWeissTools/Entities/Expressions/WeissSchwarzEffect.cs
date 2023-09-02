using Montage.Weiss.Tools.Entities.Expressions.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Entities.Expressions;

/*
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
[JsonDerivedType(typeof(GainStatEffect))]
*/
public abstract record WeissSchwarzEffect
{
}