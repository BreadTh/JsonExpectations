namespace BreadTh.DataLayoutExpectations.Error;

public record ExpectationViolations(List<ExpectationViolation> Violations)
{
    public ExpectationViolations(params ExpectationViolation[] violations)
        : this(violations.ToList())
    { }

    public ExpectationViolations(ExpectationViolation violation)
        : this(new List<ExpectationViolation> { violation })
    { }

    public ExpectationViolations(string Id, string Description)
        : this(new List<ExpectationViolation> { new ExpectationViolation(Id, Description) })
    { }

    public ExpectationViolations()
        : this(new List<ExpectationViolation>())
    { }

    public static implicit operator ExpectationViolations(ExpectationViolation violation) =>
        new ExpectationViolations(violation);

    public void Add(string id, string description) =>
        Violations.Add(new ExpectationViolation(id, description));

    public void Add(ExpectationViolation violation) =>
        Violations.Add(violation);

    public void Add(ExpectationViolations violations) =>
        Violations.AddRange(violations.Violations);

    public bool IsEmpty() =>
        Violations.Count == 0;

    public string Printable() =>
        string.Join("\n", Violations.Select(x => x.Printable()));
}
