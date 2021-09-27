using BreadTh.DataLayoutExpectations.Error;
using OneOf;
using System.Text.Json;

namespace BreadTh.DataLayoutExpectations.Interface;

public interface IJsonExpectationStatic<TSelf> where TSelf : class, IJsonExpectationStatic<TSelf>
{
    static abstract OneOf<ExpectationViolations, TSelf> From(JsonElement element, string path, bool required);
}

