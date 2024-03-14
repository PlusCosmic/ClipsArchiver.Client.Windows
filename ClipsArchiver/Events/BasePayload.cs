namespace ClipsArchiver.Events;

public class BasePayload
{
    public int RequesterId { get; set; }
    public int RecieverId { get; set; }
    public DateTime Date { get; set; }
}