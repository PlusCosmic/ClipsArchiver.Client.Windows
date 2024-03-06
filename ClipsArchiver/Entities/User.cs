namespace ClipsArchiver.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ApexUsername { get; set; }
    public string ApexUid { get; set; }
    
    public override string ToString()
    {
        return $"{Id} - {Name}";
    }
}