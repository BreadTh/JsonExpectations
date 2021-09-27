using OneOf;
using BreadTh.DataLayoutExpectations.Mcintyre321.ValueOf;
using BreadTh.DataLayoutExpectations.Interface;
using BreadTh.DataLayoutExpectations.Error;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BreadTh.DataLayoutExpectations;

public abstract class JsonStringExpectation<TSelf> : ValueOf<string, TSelf>, IJsonExpectation<TSelf> where TSelf : JsonStringExpectation<TSelf>, IValueOf<string, TSelf>, new()
{
    public static OneOf<ExpectationViolations, TSelf> From(JsonElement element, string path, bool required) =>
        element.ValueKind switch
        {
            JsonValueKind.Null => NullHandle(path, required),
            JsonValueKind.Undefined => NullHandle(path, required),
            JsonValueKind.Object => new ExpectationViolations("3533e38f-c662-4e8f-894f-4f3f7877fcc5", $"@({path}) Expected a primitive, but got an object."),
            JsonValueKind.Array => new ExpectationViolations("27e19871-a8e5-4b87-b8c9-3e8ffaba02eb", $"@({path}) Expected a primitive, but got an array."),
            _ => TSelf.From(element.ToString()).Validate(path)
        };

    protected virtual OneOf<ExpectationViolations, TSelf> Validate(string path) =>
        (TSelf)this;

    protected static OneOf<ExpectationViolations, TSelf> NullHandle(string path, bool required) =>
        required ? new ExpectationViolations("ad774497-425c-4dc1-aaf4-3693831bc14d", $"@({path}) Expected a value, but got nothing.") : (TSelf)null;

    public JsonNode ToJsonNode(string path) =>
        Value;

    public static bool operator ==(JsonStringExpectation<TSelf> wrapped, string primitive) =>
        wrapped.Value == primitive;

    public static bool operator !=(JsonStringExpectation<TSelf> wrapped, string primitive) =>
        wrapped.Value != primitive;

    public static bool operator ==(string primitive, JsonStringExpectation<TSelf> wrapped) =>
        wrapped.Value == primitive;

    public static bool operator !=(string primitive, JsonStringExpectation<TSelf> wrapped) =>
        wrapped.Value != primitive;

}
