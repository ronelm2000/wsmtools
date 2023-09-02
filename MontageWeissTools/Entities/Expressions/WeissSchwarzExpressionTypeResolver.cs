using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;
using Montage.Card.API.Entities;

namespace Montage.Weiss.Tools.Entities.Expressions;

public class WeissSchwarzExpressionTypeResolver : DefaultJsonTypeInfoResolver
{
    public static IEnumerable<JsonDerivedType> GetJsonDerivedTypes<T>()
    {
        Dictionary<object, JsonDerivedType> objects = new();
        foreach (Type type in
            (Assembly.GetAssembly(typeof(T))?.GetTypes() ?? Array.Empty<Type>())
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)) && myType.IsAssignableTo(typeof(IJsonDerivedTypeResolvable))))
        {
            var method = type.GetMethod(IJsonDerivedTypeResolvable.DerivedTypeMethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (method?.Invoke(null, null) is JsonDerivedType jsonDerivedType && jsonDerivedType.TypeDiscriminator is not null)
                objects[jsonDerivedType.TypeDiscriminator] = jsonDerivedType;
        }
        return objects.Values;
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);

        var basePointType = typeof(WeissSchwarzExpression);
        if (jsonTypeInfo.Type == typeof(WeissSchwarzCondition))
            jsonTypeInfo.PolymorphismOptions = GetRelectedPolymophismOptions<WeissSchwarzCondition>(jsonTypeInfo);
        else if (jsonTypeInfo.Type == typeof(WeissSchwarzEffect))
            jsonTypeInfo.PolymorphismOptions = GetRelectedPolymophismOptions<WeissSchwarzEffect>(jsonTypeInfo);
        else if (jsonTypeInfo.Type == typeof(WeissSchwarzTarget))
            jsonTypeInfo.PolymorphismOptions = GetRelectedPolymophismOptions<WeissSchwarzTarget>(jsonTypeInfo);
        else if (jsonTypeInfo.Type == typeof(WeissSchwarzCardContextCondition))
            jsonTypeInfo.PolymorphismOptions = GetRelectedPolymophismOptions<WeissSchwarzCardContextCondition>(jsonTypeInfo);

        return jsonTypeInfo;
    }

    private static JsonPolymorphismOptions GetRelectedPolymophismOptions<T>(JsonTypeInfo jsonTypeInfo)
    {
        var result = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "$tid",
            IgnoreUnrecognizedTypeDiscriminators = true,

            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        };
        foreach (var derivedType in GetJsonDerivedTypes<T>())
            result.DerivedTypes.Add(derivedType);
        return result;
    }
}
