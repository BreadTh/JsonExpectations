using Newtonsoft.Json.Linq;
using OneOf;
using BreadTh.DataLayoutExpectations.Mcintyre321.ValueOf;
using BreadTh.DataLayoutExpectations.Interface;
using BreadTh.DataLayoutExpectations.Error;

namespace BreadTh.DataLayoutExpectations;

public abstract class JsonStringExpectation<TSelf> : ValueOf<string, TSelf>, IJsonExpectation<TSelf> where TSelf : JsonStringExpectation<TSelf>, IValueOf<string, TSelf>, new()
{
    public static OneOf<ExpectationViolations, TSelf> From(JToken token, string path, bool required) =>
        token.Type switch
        {
            JTokenType.None => NullHandle(path, required),
            JTokenType.Null => NullHandle(path, required),
            JTokenType.Undefined => NullHandle(path, required),
            JTokenType.Object => new ExpectationViolations("3533e38f-c662-4e8f-894f-4f3f7877fcc5", $"@({path}) Expected a primitive, but got an object."),
            JTokenType.Array => new ExpectationViolations("27e19871-a8e5-4b87-b8c9-3e8ffaba02eb", $"@({path}) Expected a primitive, but got an array."),
            _ => TSelf.From(token.ToString()).Validate(path)
        };

    protected virtual OneOf<ExpectationViolations, TSelf> Validate(string path) =>
        (TSelf)this;

    protected static OneOf<ExpectationViolations, TSelf> NullHandle(string path, bool required) =>
        required ? new ExpectationViolations("ad774497-425c-4dc1-aaf4-3693831bc14d", $"@({path}) Expected a value, but got nothing.") : (TSelf)null;

    public JToken ToJToken(string path) =>
        new JValue(Value);

    public static bool operator ==(JsonStringExpectation<TSelf> wrapped, string primitive) =>
        wrapped.Value == primitive;

    public static bool operator !=(JsonStringExpectation<TSelf> wrapped, string primitive) =>
        wrapped.Value != primitive;

    public static bool operator ==(string primitive, JsonStringExpectation<TSelf> wrapped) =>
        wrapped.Value == primitive;

    public static bool operator !=(string primitive, JsonStringExpectation<TSelf> wrapped) =>
        wrapped.Value != primitive;

}
