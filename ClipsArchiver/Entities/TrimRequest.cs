namespace ClipsArchiver.Entities;

public class TrimRequest
{
    public int Id { get; set; }
    public int ClipId { get; set; }
    public string Status { get; set; }
    public NullTime StartedAt { get; set; }
    public NullTime FinishedAt { get; set; }
    public NullString ErrorMessage { get; set; }
    public NullInt DesiredStartTime { get; set; }
    public NullInt DesiredEndTime { get; set; }
}