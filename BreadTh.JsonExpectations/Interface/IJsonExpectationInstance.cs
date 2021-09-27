using Newtonsoft.Json.Linq;

namespace BreadTh.DataLayoutExpectations.Interface;

public interface IJsonExpectationInstance
{
    JToken ToJToken(string path);
}

