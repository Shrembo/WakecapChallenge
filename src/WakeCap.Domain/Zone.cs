namespace WakeCap.Domain;

public sealed class Zone
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;

    private Zone() { }
    private Zone(string name, string code)
    {
        Name = name;
        Code = code;
    }

    public static Zone Create(string name, string code)
    {
        return new Zone(name, code);
    }
}