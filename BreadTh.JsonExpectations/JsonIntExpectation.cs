using OneOf;
using BreadTh.DataLayoutExpectations.Mcintyre321.ValueOf;
using BreadTh.DataLayoutExpectations.Interface;
using BreadTh.DataLayoutExpectations.Error;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BreadTh.DataLayoutExpectations;

public abstract class JsonIntExpectation<TSelf> : ValueOf<int, TSelf>, IJsonExpectation<TSelf> where TSelf : JsonIntExpectation<TSelf>, IValueOf<int, TSelf>, new()
{
    public static OneOf<ExpectationViolations, TSelf> From(JsonElement element, string path, bool required) =>
        element.ValueKind switch
        {
            JsonValueKind.Null => NullHandle(path, required),
            JsonValueKind.Undefined => NullHandle(path, required),
            JsonValueKind.Object => new ExpectationViolations("19d96ad8-2505-490e-bf09-d5418b61f006", $"@({path}) Expected a primitive, but got an object."),
            JsonValueKind.Array => new ExpectationViolations("153f4135-acf5-45aa-98be-2024a6ef37e5", $"@({path}) Expected a primitive, but got an array."),
            _ => ParseAndValidate(element, path)
        };

    private static OneOf<ExpectationViolations, TSelf> ParseAndValidate(JsonElement element, string path)
    {
        var stringValue = element.ToString();

        if (int.TryParse(stringValue, out int result))
            return TSelf.From(result).Validate(path);
        else
            return new ExpectationViolations("6453b214-d6b7-43b5-9b47-df732d569043", $"@({path}) Expected an int, but got the value \"{stringValue}\"");
    }

    protected virtual OneOf<ExpectationViolations, TSelf> Validate(string path) =>
        (TSelf)this;

    protected static OneOf<ExpectationViolations, TSelf> NullHandle(string path, bool required) =>
        required ? new ExpectationViolations("ee803d93-37ed-4796-a4ba-3b3ada66f51a", $"@({path}) Expected a value, but got nothing.") : (TSelf)null;

    public JsonNode ToJsonNode(string path) =>
        Value;

    public static bool operator ==(JsonIntExpectation<TSelf> wrapped, int primitive) =>
        wrapped.Value == primitive;

    public static bool operator !=(JsonIntExpectation<TSelf> wrapped, int primitive) =>
        wrapped.Value != primitive;

    public static bool operator ==(int primitive, JsonIntExpectation<TSelf> wrapped) =>
        wrapped.Value == primitive;

    public static bool operator !=(int primitive, JsonIntExpectation<TSelf> wrapped) =>
        wrapped.Value != primitive;
}
