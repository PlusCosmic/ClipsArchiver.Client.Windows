namespace ClipsArchiver.Entities;

public class Settings
{
    public string ClipsPath { get; set; } = string.Empty;
    public int UserId { get; set; }
    public bool ShouldWatchClipsPath { get; set; }

    public static Settings GetDefaultSettings()
    {
        return new Settings
        {
            ClipsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) + @"\Clips\Apex Legends",
            UserId = 0,
            ShouldWatchClipsPath = false
        };
    }
}