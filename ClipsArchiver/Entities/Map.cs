namespace ClipsArchiver.Entities;

public class Map
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CardImage { get; set; }
    public string AlsName { get; set; }
    
    public override string ToString()
    {
        return Name;
    }
}