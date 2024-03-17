namespace ClipsArchiver.Entities;

public class Legend
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CardImage { get; set; }
    
    public override string ToString()
    {
        return Name;
    }
}