using System.Text.Json.Nodes;

namespace BreadTh.DataLayoutExpectations.Interface;

public interface IJsonExpectationInstance
{
    JsonNode ToJsonNode(string path);
}

