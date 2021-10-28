using OneOf;
using BreadTh.DataLayoutExpectations.Mcintyre321.ValueOf;
using BreadTh.DataLayoutExpectations.Interface;
using BreadTh.DataLayoutExpectations.Error;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace BreadTh.DataLayoutExpectations;

public abstract class JsonListExpectation<TElement, TSelf> : ValueOf<List<TElement>, TSelf>, IJsonExpectation<TSelf>
    where TElement : class, IJsonExpectation<TElement>
    where TSelf : JsonListExpectation<TElement, TSelf>, IValueOf<List<TElement>, TSelf>, new()
{
    public static OneOf<ExpectationViolations, TSelf> From(JsonElement element, string path, bool required) =>
        element.ValueKind switch
        {
            JsonValueKind.Null => NullHandle(path, required),
            JsonValueKind.Undefined => NullHandle(path, required),
            JsonValueKind.Object => new ExpectationViolations("17c32bf4-3c4d-491c-a18b-a526fc2e4468", $"@({path}) Expected an array, but got an object."),
            JsonValueKind.Array => ParseAndValidate(element, path),
            _ => new ExpectationViolations("d2fbaa82-41db-4b30-8458-bebb886d57df", $"@({path}) Expected an array, but got a primitive."),
        };

    private static OneOf<ExpectationViolations, TSelf> ParseAndValidate(JsonElement element, string path)
    {
        var result = new List<TElement>();
        var violations = new ExpectationViolations();

        var index = 0;
        foreach (var child in element.EnumerateArray()) 
        {
            TElement.From(child, $"{path}[{index}]", true)
                .Switch(
                    (ExpectationViolations newViolations) => violations.Add(newViolations),
                    (TElement childResult) => result.Add(childResult)
                );
            index++;
        }

        if (!violations.IsEmpty())
            return violations;

        return TSelf.From(result).Validate(path);
    }

    protected virtual OneOf<ExpectationViolations, TSelf> Validate(string path) =>
        (TSelf)this;

    protected static OneOf<ExpectationViolations, TSelf> NullHandle(string path, bool required) =>
        required ? new ExpectationViolations("8d0cdce8-f9f6-4f91-988c-eede0416c463", $"@({path}) Expected a value, but got nothing.") : (TSelf)null;

    public JsonNode ToJsonNode(string path)
    {
        var result = new List<JsonNode>();
        var elements = Value.ToList();

        for (var index = 0; index < elements.Count; index++)
        {
            var element = elements[index];
            var elementPath = $"{path}[{index}]";

            //No need to test if the interface exists here. The generic constraints ensures that.
            IJsonExpectationInstance jsonInterface = element;

            var token = jsonInterface.ToJsonNode(elementPath);
            result.Add(token);
        }
        return new JsonArray(result.ToArray());
    }

    public TElement this[int index]
    {
        get => Value[index];
        set => Value[index] = value;
    }
}
