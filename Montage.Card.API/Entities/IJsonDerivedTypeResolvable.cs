using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Montage.Card.API.Entities;

public interface IJsonDerivedTypeResolvable
{
    public static abstract JsonDerivedType DerivedType { get; }
    public static string DerivedTypeMethodName => "get_DerivedType";
}
