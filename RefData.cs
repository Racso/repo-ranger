public struct RefData
{
    public RefType Type { get; set; }
    public string Name { get; set; }
    public string Hash { get; set; }
}

public enum RefType
{
    Commit,
    Branch,
    Tag
}