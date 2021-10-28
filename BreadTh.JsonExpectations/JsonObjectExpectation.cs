using OneOf;
using BreadTh.DataLayoutExpectations.Interface;
using BreadTh.DataLayoutExpectations.Error;
using System.Text.Json;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;

namespace BreadTh.DataLayoutExpectations;

public interface IJsonObjectExpectationPropAccessor
{
    string GetJsonName();
    JsonNode ToJsonNode(string path);
    ExpectationViolations TryCreateAndSet(JsonElement element, string path);
}

public record class JsonObjectExpectationPropAccessor<T>(string Name, Func<T?> Getter, Action<T> Setter, bool required) : IJsonObjectExpectationPropAccessor where T : class, IJsonExpectation<T>
{
    public string GetJsonName() =>
        Name;

    public JsonNode ToJsonNode(string path) 
    {
        var instance = Getter();
        if(instance is null)
            return null!;

        var value = instance.ToJsonNode(path);
        return value;
    }

    public ExpectationViolations TryCreateAndSet(JsonElement element, string path) =>
        T.From(element, path, required).Match( 
            (ExpectationViolations violations) => violations,
            (T instance) => 
            {
                Setter(instance);
                return new ExpectationViolations();
            }
        );
}

public abstract class JsonObjectExpectation<TSelf> : IJsonRootExpectation<TSelf> where TSelf : JsonObjectExpectation<TSelf>, new()
{
    private static JsonElement jsonElementNull = JsonDocument.Parse("null").RootElement;

    protected abstract IEnumerable<IJsonObjectExpectationPropAccessor> GetPropAccessors();

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
                return (TSelf)null!;
            else
                return new ExpectationViolations("f6c9e553-16b3-4c2d-92a8-2ba9c007b16e", $"@({path}) Expected an object, but got {element.ValueKind}");
        }

        var instance = new TSelf();
        var violations = new ExpectationViolations();

        foreach(var accessor in instance.GetPropAccessors()) 
        {
            var propName = accessor.GetJsonName();
            var propFound = element.TryGetProperty(propName, out JsonElement childElement);
            var potentialViolations = accessor.TryCreateAndSet(propFound ? childElement : jsonElementNull, path);
            violations.Add(potentialViolations);
        }

        if (violations.IsEmpty())
            return instance;
        else
            return violations;
    }

    public JsonNode ToJsonNode(string path)
    {
        var result = new JsonObject();

        foreach (var accessor in GetPropAccessors())
            result.Add(accessor.GetJsonName(), accessor.ToJsonNode(path));
        
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
}
