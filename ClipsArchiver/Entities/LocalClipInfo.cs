using SQLite;

namespace ClipsArchiver.Entities;

public class LocalClipInfo
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public int ClipId { get; set; }
    public bool Watched { get; set; }
}