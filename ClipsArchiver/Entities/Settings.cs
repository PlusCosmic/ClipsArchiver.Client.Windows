namespace ClipsArchiver.Entities;

public class Settings
{
    public string ClipsPath { get; set; }
    public int UserId { get; set; }

    public static Settings GetDefaultSettings()
    {
        return new Settings
        {
            ClipsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) + @"\Clips\Apex Legends",
            UserId = 0
        };
    }
}