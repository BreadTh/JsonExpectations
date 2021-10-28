using BreadTh.DataLayoutExpectations;
using BreadTh.DataLayoutExpectations.Error;
using OneOf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

//You should always set your culture in your applications, even if you aren't using this library.
//Set it to your local CultureInfo if need be, but don't let it depend on the runtime environment.
//This example is set up for InvariantCulture.
Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

//let's make some json string example data to run JsonExpectation parser on.
//We're using a dictionary rather than an anonymous object to be able to use the specialProperty name.
string successText = JsonSerializer.Serialize(new Dictionary<string, object>
{
    { "Required", "Hi." },
    { "Excited", "Hello World!"},
    { "InternationalGreetings", new List<string> { "Hi", "Bonjour", "Hello", "Hola", "Zdravstvuyte", "N\\u01D0n h\\u01CEo", "Salve", "Konnichiwa", "Guten Tag", "Olá", "Anyoung haseyo", "Asalaam alaikum" } },
    { "Spec!al::Property", "HI!"},
    { "Inner", new { InnerMessage = "Hello" } },
    { "Option", "Def"},
    { "Integer", 7},
    { "Integer2", "42" },
    { "Number", 7 },
    { "Number2", 7.5 },
    { "Number3", "7,5" }, //This is allowed, but it will be parsed as 75.0, not 7.5
    { "Boolean", true },
    { "Boolean2", "TrUe" },
    { "Boolean3", 1 },
    { "StringifiedObject", "{\"Field\": \"Value\"}"}
});

string failText = JsonSerializer.Serialize(new Dictionary<string, object>
{
    { "Excited", "Hello World" },
    { "InternationalGreetings", new List<string> { "Hi", "Hello", "Hey", "What's up" }},
    { "SpecialProperty", "HI!"},
    { "Option", "Xyz"},
    { "Integer", 7.5},
    { "Integer2", "Hello" },
    { "Number", null },
    { "Number2", "1.000.000,5" },
    { "Number3", "1.000.000.5" },
    { "Boolean", "Hello" },
    { "Boolean2", "true." },
    { "Boolean3", 2 },
    { "StringifiedObject", "{Field: Value}"}
});

//Now to actually run the parsing
Console.WriteLine("Parsing successText:");
Print(MyModel.FromJsonString(successText));
Console.WriteLine("\nParsing failText:");
Print(MyModel.FromJsonString(failText));
//Console.ReadLine();

//The result of FromJsonString is a "OneOf". OneOf is a representation of multiple outcomes.
//In this case its used to return either information about what went wrong when parsing the model,
//or the successfully parsed model itself.
//.Match can also be used instead of .Switch if you want to return something regardless of the outcome.
//More info here: https://github.com/mcintyre321/OneOf
void Print(OneOf<ExpectationViolations, MyModel> outcome) =>
    outcome.Switch(
        (ExpectationViolations violations) => Console.WriteLine("\t"+violations.Printable().Replace("\n", "\n\t")),
        (MyModel result) => PrintIfInnerMessageIsHello(result)
    );

void PrintIfInnerMessageIsHello(MyModel result)
{
    //We now have access to the parsed model. All wrappers have been overloaded so you can compare
    //as if they were their corresponding primitive.
    if(result.Excited == "Hello World!")
        //likewise when we want to serialized, the result will come out as if was just a bunch of primitives.
        Console.WriteLine("\t" + result.ToJsonString(true).Replace("\n", "\n\t"));
    
    //Both properly escaped and not escaped characters are read and printed correctly (You may need to adjust your console settings for them to display, though!)
    Console.WriteLine("\t"+result.InternationalGreetings[5]);
    Console.WriteLine("\t"+result.InternationalGreetings[9]);
    Console.Write('\u2103'); //℃ character code
}


public class MyModel : JsonObjectExpectation<MyModel>
{
    public Message? Optional { get; set; }
    
    [Required]
    public Message Required { get; set; } = null!;

    [Required]
    public ExcitedMessage Excited { get; set; } = null!;

    [Required]
    public InternationalGreetingList InternationalGreetings { get; set; } = null!;

    public class InternationalGreetingList : JsonListExpectation<Greeting, InternationalGreetingList>
    {
        protected override OneOf<ExpectationViolations, InternationalGreetingList> Validate(string path) =>
            Value.All(greeting => greeting.IsEnglishCommonGreeting())
                ? new ExpectationViolations("4805ad75-371c-433e-ba81-e2ea6d36e973", $"@({path}) English greetings are allowed in an InternationalGreetings set, but at least one non-English greeting is required.")
                : this;
    }

    [Required]
    [JsonPropertyName("Spec!al::Property")]
    public Message SpecialProperty { get; set; } = null!;

    [Required]
    public InnerObject Inner { get; set; } = null!;
 
    [Required]
    public Option Option { get; set; } = null!;

    [Required]
    public MyInt Integer { get; set; } = null!;

    [Required]
    public MyInt Integer2 { get; set; } = null!;


    [Required]
    public MyDouble Number { get; set; } = null!;

    [Required]
    public MyDouble Number2 { get; set; } = null!;

    [Required]
    public MyDouble Number3 { get; set; } = null!;

    [Required]
    public MyBool Boolean { get; set; } = null!;

    [Required]
    public MyBool Boolean2 { get; set; } = null!;
    
    [Required]
    public MyBool Boolean3 { get; set; } = null!;

    [Required]
    public StringifiedObject StringifiedObject { get; set; } = null!;

    protected override IEnumerable<IJsonObjectExpectationPropAccessor> GetPropAccessors() =>
        new List<IJsonObjectExpectationPropAccessor>()
        {   new JsonObjectExpectationPropAccessor<Message>("Optional", () => Optional, value => Optional = value, false)
        ,   new JsonObjectExpectationPropAccessor<Message>("Required", () => Required, value => Required = value, true)
        ,   new JsonObjectExpectationPropAccessor<ExcitedMessage>("Excited", () => Excited, value => Excited = value, true)
        ,   new JsonObjectExpectationPropAccessor<InternationalGreetingList>("InternationalGreetings", () => InternationalGreetings, value => InternationalGreetings = value, true)
        ,   new JsonObjectExpectationPropAccessor<Message>("Spec!al::Property", () => SpecialProperty, value => SpecialProperty = value, true)
        ,   new JsonObjectExpectationPropAccessor<InnerObject>("Inner", () => Inner, value => Inner = value, true)
        ,   new JsonObjectExpectationPropAccessor<Option>("Option", () => Option, value => Option = value, true)
        ,   new JsonObjectExpectationPropAccessor<MyInt>("Integer", () => Integer, value => Integer = value, true)
        ,   new JsonObjectExpectationPropAccessor<MyInt>("Integer2", () => Integer2, value => Integer2 = value, true)
        ,   new JsonObjectExpectationPropAccessor<MyDouble>("Number", () => Number, value => Number = value, true)
        ,   new JsonObjectExpectationPropAccessor<MyDouble>("Number2", () => Number2, value => Number2 = value, true)
        ,   new JsonObjectExpectationPropAccessor<MyDouble>("Number3", () => Number3, value => Number3 = value, true)
        ,   new JsonObjectExpectationPropAccessor<MyBool>("Boolean", () => Boolean, value => Boolean = value, true)
        ,   new JsonObjectExpectationPropAccessor<MyBool>("Boolean2", () => Boolean2, value => Boolean2 = value, true)
        ,   new JsonObjectExpectationPropAccessor<MyBool>("Boolean3", () => Boolean3, value => Boolean3 = value, true)
        ,   new JsonObjectExpectationPropAccessor<StringifiedObject>("StringifiedObject", () => StringifiedObject, value => StringifiedObject = value, true)
        };

}

public class Message : JsonStringExpectation<Message> { }

public class ExcitedMessage : JsonStringExpectation<ExcitedMessage>
{
    protected override OneOf<ExpectationViolations, ExcitedMessage> Validate(string path) =>
        Value.Contains('!') ? this : new ExpectationViolations("361ff365-2ebf-4df2-8ea9-eca52ec0c444", $"@({path}) Only excited greetings are allowed!!!");
}

public class Greeting : JsonStringExpectation<Greeting>
{
    public bool IsEnglishCommonGreeting() =>
        (new List<string> { "hello", "hi", "hey", "what's up" }).Contains(Value.ToLower());
}

public class InnerObject : JsonObjectExpectation<InnerObject>
{
    [Required]
    public Message InnerMessage { get; set; } = null!;

    protected override IEnumerable<IJsonObjectExpectationPropAccessor> GetPropAccessors() =>
        new List<IJsonObjectExpectationPropAccessor>()
        {   new JsonObjectExpectationPropAccessor<Message>("InnerMessage", () => InnerMessage, value => InnerMessage = value, true)        
        };
}

//the enum does not have to be nested.
public class Option : JsonEnumExpectation<Option.PossibleValues, Option>
{
    public enum PossibleValues { Abc, Def, Hij }
}

public class MyInt : JsonIntExpectation<MyInt> { }
public class MyDouble : JsonDoubleExpectation<MyDouble> { }
public class MyBool : JsonBoolExpectation<MyBool> { }
public class StringifiedObject : JsonObjectExpectation<StringifiedObject> 
{
    [Required]
    public MyString Field { get; set; } = null!;

    protected override IEnumerable<IJsonObjectExpectationPropAccessor> GetPropAccessors() =>
        new List<IJsonObjectExpectationPropAccessor>()
        {   new JsonObjectExpectationPropAccessor<MyString>("Field", () => Field, value => Field = value, true)
        };
}

public class MyString : JsonStringExpectation<MyString> { }