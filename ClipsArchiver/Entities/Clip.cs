using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClipsArchiver.Services;

namespace ClipsArchiver.Entities;

public class Clip
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string? Filename { get; set; }
    public bool IsProcessed { get; set; }
    public NullTime? CreatedOn { get; set; }
    public int Duration { get; set; }
    public NullInt Map { get; set; }
    public NullString? GameMode { get; set; }
    public NullInt Legend { get; set; }
    public bool MatchHistoryFound { get; set; }
    public List<string> Tags { get; set; } = new();
    public string VideoUri { get; set; } = string.Empty;
    public string ThumbnailUri { get; set; } = string.Empty;
}