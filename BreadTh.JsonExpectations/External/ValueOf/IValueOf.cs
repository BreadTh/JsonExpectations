namespace BreadTh.DataLayoutExpectations.Mcintyre321.ValueOf
{
    public interface IValueOfStatic<TValue, TThis> 
    {
        static abstract TThis From(TValue item);
    }

    public interface IValueOfInstance<TValue>
    {
        TValue Value { get; }
        bool Equals(object obj);
        int GetHashCode();
        string ToString();
    }

    public interface IValueOf<TValue, TThis> : IValueOfStatic<TValue, TThis>, IValueOfInstance<TValue> { }
}