namespace ClipsArchiver.Entities;

public class QueueEntry
{
    public int Id { get; set; }
    public int ClipId { get; set; }
    public string? Status { get; set; }
    public NullTime StartedAt { get; set; } = new();
    public NullTime FinishedAt { get; set; } = new();
}