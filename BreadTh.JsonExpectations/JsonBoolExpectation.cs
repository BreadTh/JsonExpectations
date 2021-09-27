using Newtonsoft.Json.Linq;
using OneOf;
using BreadTh.DataLayoutExpectations.Mcintyre321.ValueOf;
using BreadTh.DataLayoutExpectations.Interface;
using BreadTh.DataLayoutExpectations.Error;

namespace BreadTh.DataLayoutExpectations;

public abstract class JsonBoolExpectation<TSelf> : ValueOf<bool, TSelf>, IJsonExpectation<TSelf>
    where TSelf : JsonBoolExpectation<TSelf>, IValueOf<bool, TSelf>, new()
{
    public static OneOf<ExpectationViolations, TSelf> From(JToken token, string path, bool required) =>
        token.Type switch
        {
            JTokenType.None => NullHandle(path, required),
            JTokenType.Null => NullHandle(path, required),
            JTokenType.Undefined => NullHandle(path, required),
            JTokenType.Object => new ExpectationViolations("5b79061f-4e69-4d3b-80f3-839cd262d05f", $"@({path}) Expected a primitive, but got an object."),
            JTokenType.Array => new ExpectationViolations("df252cff-fcf0-4c0d-a61a-b0e7bccf473b", $"@({path}) Expected a primitive, but got an array."),
            _ => ParseAndValidate(token, path)
        };

    private static OneOf<ExpectationViolations, TSelf> ParseAndValidate(JToken token, string path)
    {
        var stringValue = token.ToString();

        return stringValue.ToLower() switch
        {
            "false" => TSelf.From(false).Validate(path),
            "0" => TSelf.From(false).Validate(path),
            "true" => TSelf.From(true).Validate(path),
            "1" => TSelf.From(true).Validate(path),
            _ => new ExpectationViolations("b17fbb36-e441-422c-afae-8f19ef2159df", $"@({path}) Expected a boolean, but got the value \"{stringValue}\"")
        };
    }

    protected virtual OneOf<ExpectationViolations, TSelf> Validate(string path) =>
        (TSelf)this;

    protected static OneOf<ExpectationViolations, TSelf> NullHandle(string path, bool required) =>
        required ? new ExpectationViolations("b09fbdba-b96b-46ad-8e0f-409d0a5da578", $"@({path}) Expected a value, but got nothing.") : (TSelf)null;

    public JToken ToJToken(string path) =>
        new JValue(Value);

    public static bool operator ==(JsonBoolExpectation<TSelf> wrapped, bool primitive) =>
        wrapped.Value == primitive;

    public static bool operator !=(JsonBoolExpectation<TSelf> wrapped, bool primitive) =>
        wrapped.Value != primitive;

    public static bool operator ==(bool primitive, JsonBoolExpectation<TSelf> wrapped) =>
        wrapped.Value == primitive;

    public static bool operator !=(bool primitive, JsonBoolExpectation<TSelf> wrapped) =>
        wrapped.Value != primitive;
}
