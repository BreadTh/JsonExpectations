using BreadTh.DataLayoutExpectations.Error;
using OneOf;

namespace BreadTh.DataLayoutExpectations.Interface;

public interface IJsonRootExpectation<TSelf> : IJsonExpectation<TSelf> where TSelf : class, IJsonExpectation<TSelf>
{
    static abstract OneOf<ExpectationViolations, TSelf> FromJsonString(string rawJson);

    string ToJsonString(bool indented);
}