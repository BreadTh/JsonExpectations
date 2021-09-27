using Newtonsoft.Json.Linq;
using OneOf;
using BreadTh.DataLayoutExpectations.Mcintyre321.ValueOf;
using BreadTh.DataLayoutExpectations.Interface;
using BreadTh.DataLayoutExpectations.Error;

namespace BreadTh.DataLayoutExpectations;

public abstract class JsonListExpectation<TElement, TSelf> : ValueOf<IEnumerable<TElement>, TSelf>, IJsonExpectation<TSelf>
    where TElement : class, IJsonExpectation<TElement>
    where TSelf : JsonListExpectation<TElement, TSelf>, IValueOf<IEnumerable<TElement>, TSelf>, new()
{
    public static OneOf<ExpectationViolations, TSelf> From(JToken token, string path, bool required) =>
        token.Type switch
        {
            JTokenType.None => NullHandle(path, required),
            JTokenType.Null => NullHandle(path, required),
            JTokenType.Undefined => NullHandle(path, required),
            JTokenType.Object => new ExpectationViolations("17c32bf4-3c4d-491c-a18b-a526fc2e4468", $"@({path}) Expected an array, but got an object."),
            JTokenType.Array => ParseAndValidate((JArray)token, path),
            _ => new ExpectationViolations("d2fbaa82-41db-4b30-8458-bebb886d57df", $"@({path}) Expected an array, but got a primitive."),
        };

    private static OneOf<ExpectationViolations, TSelf> ParseAndValidate(JArray jArray, string path)
    {
        var result = new List<TElement>();
        var violations = new ExpectationViolations();

        var children = jArray.Children().ToArray();

        for (var index = 0; index < children.Count(); index++)
            TElement.From(children[index] ?? JValue.CreateNull(), $"{path}[{index}]", true)
                .Switch(
                    (ExpectationViolations newViolations) => violations.Add(newViolations),
                    (TElement childResult) => result.Add(childResult)
                );

        if (!violations.IsEmpty())
            return violations;

        return TSelf.From(result).Validate(path);
    }

    protected virtual OneOf<ExpectationViolations, TSelf> Validate(string path) =>
        (TSelf)this;

    protected static OneOf<ExpectationViolations, TSelf> NullHandle(string path, bool required) =>
        required ? new ExpectationViolations("8d0cdce8-f9f6-4f91-988c-eede0416c463", $"@({path}) Expected a value, but got nothing.") : (TSelf)null;

    public JToken ToJToken(string path)
    {
        var result = new JArray();
        var elements = Value.ToList();

        for (var index = 0; index < elements.Count; index++)
        {
            var element = elements[index];
            var elementPath = $"{path}[{index}]";

            //No need to test if the interface exists here. The generic constraints ensures that.
            IJsonExpectationInstance jsonInterface = element;

            var token = jsonInterface.ToJToken(elementPath);
            result.Add(token);
        }
        return result;
    }
}
