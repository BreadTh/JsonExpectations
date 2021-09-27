using BreadTh.DataLayoutExpectations.Error;
using Newtonsoft.Json.Linq;
using OneOf;

namespace BreadTh.DataLayoutExpectations.Interface;

public interface IJsonExpectationStatic<TSelf> where TSelf : class, IJsonExpectationStatic<TSelf>
{
    static abstract OneOf<ExpectationViolations, TSelf> From(JToken token, string path, bool required);
}

