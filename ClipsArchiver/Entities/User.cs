namespace ClipsArchiver.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApexUsername { get; set; } = string.Empty;
    public string ApexUid { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Id} - {Name}";
    }
}