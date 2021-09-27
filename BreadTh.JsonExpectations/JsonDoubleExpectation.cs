using OneOf;
using BreadTh.DataLayoutExpectations.Mcintyre321.ValueOf;
using BreadTh.DataLayoutExpectations.Interface;
using System.Globalization;
using BreadTh.DataLayoutExpectations.Error;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace BreadTh.DataLayoutExpectations;

public abstract class JsonDoubleExpectation<TSelf> : ValueOf<double, TSelf>, IJsonExpectation<TSelf> where TSelf : JsonDoubleExpectation<TSelf>, IValueOf<double, TSelf>, new()
{
    public static OneOf<ExpectationViolations, TSelf> From(JsonElement element, string path, bool required) =>
        element.ValueKind switch
        {
            JsonValueKind.Null => NullHandle(path, required),
            JsonValueKind.Undefined => NullHandle(path, required),
            JsonValueKind.Object => new ExpectationViolations("79574ce9-ca81-432f-891b-dfbbf6220412", $"@({path}) Expected a primitive, but got an object."),
            JsonValueKind.Array => new ExpectationViolations("ca48ed16-52ac-49b7-9d09-a5bec3175663", $"@({path}) Expected a primitive, but got an array."),
            _ => ParseAndValidate(element, path)
        };

    private static OneOf<ExpectationViolations, TSelf> ParseAndValidate(JsonElement element, string path)
    {
        var stringValue = element.ToString();

        if (double.TryParse(stringValue, out double result))
            return TSelf.From(result).Validate(path);
        else
            return new ExpectationViolations("0f9c127a-b5d4-40b8-a94e-c88e2f5b92c0", $"@({path}) Expected a double, but got the value \"{stringValue}\"");
    }

    protected virtual OneOf<ExpectationViolations, TSelf> Validate(string path) =>
        (TSelf)this;

    protected static OneOf<ExpectationViolations, TSelf> NullHandle(string path, bool required) =>
        required ? new ExpectationViolations("f6d42808-57e1-496b-b737-9fc05b47d53b", $"@({path}) Expected a value, but got nothing.") : (TSelf)null;

    public JsonNode ToJsonNode(string path) =>
        Value;

    public static bool operator ==(JsonDoubleExpectation<TSelf> wrapped, double primitive) =>
        wrapped.Value == primitive;

    public static bool operator !=(JsonDoubleExpectation<TSelf> wrapped, double primitive) =>
        wrapped.Value != primitive;

    public static bool operator ==(double primitive, JsonDoubleExpectation<TSelf> wrapped) =>
        wrapped.Value == primitive;

    public static bool operator !=(double primitive, JsonDoubleExpectation<TSelf> wrapped) =>
        wrapped.Value != primitive;
}
