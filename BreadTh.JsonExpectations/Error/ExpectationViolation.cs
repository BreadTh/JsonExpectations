namespace BreadTh.DataLayoutExpectations.Error;

public record ExpectationViolation(string Id, string Description) 
{
    public string Printable() =>
        $"[{Id}] {Description}";
}