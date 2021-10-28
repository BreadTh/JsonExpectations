using System;
using System.Collections.Generic;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BreadTh.DataLayoutExpectations;
using BreadTh.DataLayoutExpectations.Error;
using OneOf;

_ = BenchmarkRunner.Run<Benchmark>();
Console.ReadLine();
public class Benchmark
{
    private readonly string stringData;
    private readonly EncapsulatedModel encapsulatedModel;
    private readonly PrimitiveModel primitiveModel;

    public Benchmark()
    {
        stringData = JsonSerializer.Serialize(new { prop1 = "Hello", prop2 = "World", prop3 = "!" });
        encapsulatedModel = EncapsulatedModel.FromJsonString(stringData).AsT1;
        primitiveModel = JsonSerializer.Deserialize<PrimitiveModel>(stringData)!;
    }

    [Benchmark]
    public PrimitiveModel? Deserialize_Primitive() =>
       JsonSerializer.Deserialize<PrimitiveModel>(stringData);

    [Benchmark]
    public OneOf<ExpectationViolations, EncapsulatedModel> Deserialize_Encapsulated() =>
        EncapsulatedModel.FromJsonString(stringData);

    [Benchmark]
    public string Serialize_Primitive() =>
        JsonSerializer.Serialize(primitiveModel);

    [Benchmark]
    public string Serialize_Encapsulated() =>
        encapsulatedModel.ToJsonString();
}

public class PrimitiveModel
{
    public string prop1 { get; set; } = null!;
    public string prop2 { get; set; } = null!;
    public string prop3 { get; set; } = null!;
}

public class EncapsulatedModel : JsonObjectExpectation<EncapsulatedModel>
{
    public Prop prop1 { get; set; } = null!;
    public Prop prop2 { get; set; } = null!;
    public OtherProp prop3 { get; set; } = null!;

    protected override IEnumerable<IJsonObjectExpectationPropAccessor> GetPropAccessors() =>
        new List<IJsonObjectExpectationPropAccessor>()
        {   new JsonObjectExpectationPropAccessor<Prop>("prop1", () => prop1, value => prop1 = value, true)
        ,   new JsonObjectExpectationPropAccessor<Prop>("prop2", () => prop2, value => prop2 = value, false)
        ,   new JsonObjectExpectationPropAccessor<OtherProp>("prop3", () => prop3, value => prop3 = value, true)
        };
}

public class Prop : JsonStringExpectation<Prop> { }
public class OtherProp : JsonStringExpectation<OtherProp> { }