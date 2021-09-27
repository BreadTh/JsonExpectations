using Apparatus.AOT.Reflection;
using BreadTh.DataLayoutExpectations;
using BreadTh.DataLayoutExpectations.Error;
using OneOf;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

//You should always set your culture in your applications, even if you aren't using this library.
//Set it to your local CultureInfo if need be, but don't let it depend on the runtime environment.
//This example iss set up for InvariantCulture.
Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

//Apparatus.AOT.Reflection lets us do compiletime reflection, meaning that we get reflection magic but without the runtime overhead.
//But there's a catch: The source generators are still a little limited in what they can "know" about at compile time,
//so the following call is essencially a helper for the source generator. Think of it as registering the model for compiletime reflection.
//The function does not actually need to be called, just included somewhere in the source code with the concrete type as the generic parameter,
//but to keep the source generators' magic from being less mystical, one might as well call the function.
//Another option is to put the call on an unused method on the object itself, as is done with MyModel.InnerObject
//If the registration is omitted, the system will fall back on runtime reflection. It'll still work, but it'll be slower.
//Not really a big deal unless your application has to be very performant.
//For more information, check out: https://github.com/byme8/Apparatus.AOT.Reflection
GenericHelper.Bootstrap<MyModel>();
GenericHelper.Bootstrap<StringifiedObject>();

//let's make some json string example data to run JsonExpectation parser on.
//We're using a dictionary rather than an anonymous object to be able to use the specialProperty name.
string successText = JsonSerializer.Serialize(new Dictionary<string, object>
{
    { "Required", "Hi." },
    { "Excited", "Hello World!"},
    { "InternationalGreetings", new List<string> { "Hi", "Bonjour", "Hello", "Hola", "Zdravstvuyte", "Nǐn hǎo", "Salve", "Konnichiwa", "Guten Tag", "Olá", "Anyoung haseyo", "Asalaam alaikum" } },
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
Console.ReadLine();

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

    public void BootstrapTypeForAotSourceGenerator() =>
        GenericHelper.Bootstrap<InnerObject>();
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
}

public class MyString : JsonStringExpectation<MyString> { }