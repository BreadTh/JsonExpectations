using Apparatus.AOT.Reflection;
using OneOf;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BreadTh.DataLayoutExpectations.Interface;
using BreadTh.DataLayoutExpectations.Error;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BreadTh.DataLayoutExpectations;

public abstract class JsonObjectExpectation<TSelf> : IJsonRootExpectation<TSelf> where TSelf : JsonObjectExpectation<TSelf>, new()
{
    public static OneOf<ExpectationViolations, TSelf> FromJsonString(string rawJson)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(rawJson);
        }
        catch
        {
            return new ExpectationViolations("8923ed7e-6775-4351-8cac-3299e3ed3c1a", "@(root) The given string is not a valid json object");
        }

        var result = From(document.RootElement, "", true)
            .Match<OneOf<ExpectationViolations, TSelf>>(
                (ExpectationViolations violations) => violations,
                (TSelf value) => value
        );

        document.Dispose();
        return result;
    }

    public static OneOf<ExpectationViolations, TSelf> From(JsonElement element, string path, bool required)
    {
        if (element.ValueKind != JsonValueKind.Object) 
        {
            if(element.ValueKind == JsonValueKind.String)
                return FromStringifiedObject(element.ToString(), path, required);
            else if (!required && element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                return (TSelf)null;
            else
                return new ExpectationViolations("f6c9e553-16b3-4c2d-92a8-2ba9c007b16e", $"@({path}) Expected an object, but got {element.ValueKind}");
        }

        var instance = new TSelf();
        var violations = new ExpectationViolations();

        var properties = GetProperties();

        foreach (var prop in properties)
        {
            string? propNameInJson = GetPropName(prop);
            string propPath = path + "." + propNameInJson;
            
            JsonElement propElement;
            if (!element.TryGetProperty(propNameInJson, out propElement))
                propElement = new JsonElement();

            if (GetInterfaceOfProp<IJsonExpectation<TSelf>>(prop) is null)
                throw new Exception($"@({propPath}) Implementations of {typeof(JsonObjectExpectation<>).Name}'s properties must implement the interface {typeof(IJsonExpectation<>).Name}.");

            MethodInfo? fromMethod = GetMethodOfProp(prop, "From", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, new[] { typeof(JsonElement), typeof(string), typeof(bool) });

            if (fromMethod is null)
                throw new Exception($"This shouldn't happen. What'd you do? The {typeof(IJsonExpectation<>).Name} interface was found, but the public static From(JsonElement, string, bool) was not.");

            var propRequired = GetPropAttributes(prop).Any((Attribute attribute) => attribute is RequiredAttribute);

            dynamic? valueOrViolation = fromMethod.Invoke(null, new object[] { propElement, propPath, propRequired });

            if (valueOrViolation is null)
                throw new Exception("From(JToken, String) must always return a value. Null is not allowed.");

            var SwitchMethod = valueOrViolation.GetType().GetMethod("Switch", BindingFlags.Public | BindingFlags.Instance, new[] { typeof(Action<ExpectationViolations>), typeof(Action<object>) });

            if (SwitchMethod is null)
                throw new Exception("This shouldn't happen. What'd you do? public static From(JToken, String)'s return value did not have a public Switch(Action<JsonExpectationViolations>, Action<object>) method");

            //Can this be done with AOT reflection? Again Method.Invoke, but on an instance this time
            SwitchMethod.Invoke(valueOrViolation, new object[] {
                new Action<ExpectationViolations>((ExpectationViolations newViolations) => violations.Add(newViolations)),
                new Action<object>((object value) => SetPropValue(prop, instance, value, propPath))
            });
        }

        if (violations.IsEmpty())
            return instance;
        else
            return violations;
    }

    public JsonNode ToJsonNode(string path)
    {
        var result = new JsonObject();

        foreach (var prop in GetProperties())
        {
            var propNameInJson = GetPropName(prop);

            var propPath = $"{path}.{propNameInJson}";

            var propValue = GetPropValue(prop, (TSelf)this);

            if (propValue is null)
                continue;

            //Note that IJsonExpectation<> extends IJsonExpectationInstance
            if (propValue is not IJsonExpectationInstance jsonInterface)
                throw new Exception($"@({propPath}) All implementations of {typeof(JsonObjectExpectation<>).GetGenericTypeDefinition().Name.Replace("`1", "<OuterType>")} properties' must implement the interface {typeof(IJsonExpectation<>).GetGenericTypeDefinition().Name.Replace("`1", "<InnerType>")}.");

            var propValueAsJToken = jsonInterface.ToJsonNode(propPath);
            result.Add(propNameInJson, propValueAsJToken);
        }

        return result;
    }

    public string ToJsonString(bool indented = false) =>
        ToJsonNode("").ToJsonString(new JsonSerializerOptions(){ WriteIndented = indented });

    private static OneOf<ExpectationViolations, TSelf> FromStringifiedObject(string stringObject, string path, bool required)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(stringObject);
        }
        catch
        {
            return new ExpectationViolations("c0e13aee-32df-441e-a956-30626926358e", $"@({path}) Expect an object, but was given a string (which could not be deserialized to an object)");
        }

        var result = From(document.RootElement, path, required)
            .Match<OneOf<ExpectationViolations, TSelf>>(
                (ExpectationViolations violations) => violations,
                (TSelf value) => value
        );

        document.Dispose();
        return result;
    }

    private static IEnumerable<OneOf<Apparatus.AOT.Reflection.IPropertyInfo, System.Reflection.PropertyInfo>> GetProperties()
    {
        bool registeredForAotReflection = MetadataStore<TSelf>.Data is not null;

        if (registeredForAotReflection)
            return AOTReflection.GetProperties<TSelf>().Values
                .Select(item => OneOf<IPropertyInfo, PropertyInfo>.FromT0(item));
        else
            return typeof(TSelf).GetProperties()
                .Select(item => OneOf<IPropertyInfo, PropertyInfo>.FromT1(item));
    }

    private static string GetPropName(OneOf<IPropertyInfo, PropertyInfo> prop) =>
        prop.Match(
            (IPropertyInfo aotProp) =>
            {
                Attribute? jsonPropertyAttribute = aotProp.Attributes.FirstOrDefault(attribute => attribute.GetType() == typeof(JsonPropertyNameAttribute));
                return jsonPropertyAttribute is null ? aotProp.Name : (((JsonPropertyNameAttribute)jsonPropertyAttribute).Name ?? aotProp.Name);
            },
            (PropertyInfo runtimeProp) =>
            {
                object? jsonPropertyAttribute = runtimeProp.GetCustomAttributes(true).FirstOrDefault(attribute => attribute.GetType() == typeof(JsonPropertyNameAttribute));
                return jsonPropertyAttribute is null ? runtimeProp.Name : (((JsonPropertyNameAttribute)jsonPropertyAttribute).Name ?? runtimeProp.Name);
            });


    private static Type? GetInterfaceOfProp<TInterface>(OneOf<IPropertyInfo, PropertyInfo> prop) =>
        prop.Match(
            //Can this be done with AOT reflection without going down the type.GetInterface route? Is it worth it?
            (IPropertyInfo aotProp) => aotProp.PropertyType,
            (PropertyInfo runtimeProp) => runtimeProp.PropertyType
        ).GetInterface(typeof(IJsonExpectation<>).GetGenericTypeDefinition().Name);

    private static MethodInfo? GetMethodOfProp(OneOf<IPropertyInfo, PropertyInfo> prop, string name, BindingFlags bindingAttr, Type[] types) =>
        prop.Match(
            //Can this be done with AOT reflection without going down the type.GetInterface route? Is it worth it?
            (IPropertyInfo aotProp) => aotProp.PropertyType,
            (PropertyInfo runtimeProp) => runtimeProp.PropertyType
        ).GetMethod(name, bindingAttr, types);

    private static Attribute?[] GetPropAttributes(OneOf<IPropertyInfo, PropertyInfo> prop) =>
        prop.Match(
            (IPropertyInfo aotProp) => aotProp.Attributes,
            (PropertyInfo runtimeProp) => runtimeProp.GetCustomAttributes(true).Select(attribute => attribute as Attribute).ToArray()
        );

    private static void SetPropValue(OneOf<IPropertyInfo, PropertyInfo> prop, TSelf instance, object value, string propPath) =>
        prop.Switch(
            (IPropertyInfo aotProp) =>
            {
                if (value is not null) //aotProp.TrySetValue throws if value is null, but null-value is valid behaviour if the field is not required.
                    if (!aotProp.TrySetValue(instance, value))
                        throw new Exception($"This shouldn't happen. What'd you do? Could not set {propPath} to {value}");
            },
            (PropertyInfo runtimeProp) =>
                runtimeProp.SetValue(instance, value)
        );

    private static object? GetPropValue(OneOf<IPropertyInfo, PropertyInfo> prop, TSelf instance) =>
        prop.Match(
            (IPropertyInfo aotProp) =>
            {
                if (aotProp.TryGetValue(instance, out object value))
                    return value;
                else
                    return null;
            },
            (PropertyInfo runtimeProp) =>
                runtimeProp.GetValue(instance)
        );
}
