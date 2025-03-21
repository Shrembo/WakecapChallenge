namespace WakeCap.Domain;

public sealed class Worker
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;

    private Worker() { }
    private Worker(string name, string code)
    {
        Name = name;
        Code = code;
    }

    public static Worker Create(string name, string code)
    {
        return new Worker(name, code);
    }
}