namespace BreadTh.DataLayoutExpectations.Interface;


public interface IJsonExpectation<TSelf> : IJsonExpectationStatic<TSelf>, IJsonExpectationInstance where TSelf : class, IJsonExpectation<TSelf>
{
}

