using OneOf;
using BreadTh.DataLayoutExpectations.Mcintyre321.ValueOf;
using BreadTh.DataLayoutExpectations.Interface;
using BreadTh.DataLayoutExpectations.Error;
using System.Text.Json;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;

namespace BreadTh.DataLayoutExpectations;

public abstract class JsonEnumExpectation<TEnum, TSelf> : ValueOf<TEnum, TSelf>, IJsonExpectation<TSelf>
    where TEnum : struct, Enum
    where TSelf : JsonEnumExpectation<TEnum, TSelf>, IValueOf<TEnum, TSelf>, new()
{
    public static OneOf<ExpectationViolations, TSelf> From(JsonElement element, string path, bool required) =>
        element.ValueKind switch
        {
            JsonValueKind.Null => NullHandle(path, required),
            JsonValueKind.Undefined => NullHandle(path, required),
            JsonValueKind.Object => new ExpectationViolations("687f4f26-6bab-4bb9-b1ab-6666616c1391", $"@({path}) Expected a primitive, but got an object."),
            JsonValueKind.Array => new ExpectationViolations("866494b5-161a-4838-9e95-d3cde1039008", $"@({path}) Expected a primitive, but got an array."),
            JsonValueKind.String => ParseAndValidate(element, path),
            _ => new ExpectationViolations("388c23bb-fd68-4f01-b93e-df4ce41b3c65", $"@({path}) Expected a string, but got another primitive. ({element.ValueKind})"),
        };

    private static OneOf<ExpectationViolations, TSelf> ParseAndValidate(JsonElement element, string path)
    {
        var stringValue = element.ToString();

        //Can we use AOT reflection here? For enum parsing.
        if (!Enum.TryParse<TEnum>(stringValue, out var value))
        {
            //Can we use AOT reflection here? IT seems registering the enum type does not help it out of the generic problems. E.g:
            //var validValues = EnumHelper.GetEnumInfo<TEnum>().Select(enumItem => enumItem.Name);

            var validValues = Enum.GetValues<TEnum>();
            return new ExpectationViolations("9114b3ca-613e-4957-b198-34273562c88d", $"@({path}) The string value (\"{stringValue}\") was not in list of valid options: [\"{string.Join("\", \"", validValues)}\"]");
        }

        return TSelf.From(value).Validate(path);
    }

    protected virtual OneOf<ExpectationViolations, TSelf> Validate(string path) =>
        (TSelf)this;

    protected static OneOf<ExpectationViolations, TSelf> NullHandle(string path, bool required) =>
        required ? new ExpectationViolations("6cd75a54-cd87-4fc7-8e29-cfb32fdb057a", $"@({path}) Expected a value, but got nothing.") : (TSelf)null;

    public JsonNode ToJsonNode(string path) =>
        //Like above, the generic registration for AOT reflection does not seem to work as well for enums. So we can't just do the following:
        //new JValue(Value.GetEnumValueInfo().Name);
        //instead we have to do Value.ToString() which has a deceptively large overhead because Value is an enum, but what else can we do?
        Value.ToString();

    public static bool operator ==(JsonEnumExpectation<TEnum, TSelf> wrapped, TEnum primitive) =>
        EqualityComparer<TEnum>.Default.Equals(wrapped.Value, primitive);

    public static bool operator !=(JsonEnumExpectation<TEnum, TSelf> wrapped, TEnum primitive) =>
        EqualityComparer<TEnum>.Default.Equals(wrapped.Value, primitive);

    public static bool operator ==(TEnum primitive, JsonEnumExpectation<TEnum, TSelf> wrapped) =>
        EqualityComparer<TEnum>.Default.Equals(wrapped.Value, primitive);

    public static bool operator !=(TEnum primitive, JsonEnumExpectation<TEnum, TSelf> wrapped) =>
        EqualityComparer<TEnum>.Default.Equals(wrapped.Value, primitive);
}
