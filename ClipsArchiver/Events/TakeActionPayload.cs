namespace ClipsArchiver.Events;

public class TakeActionPayload: BasePayload
{
    public Constants.Action Action { get; set; }
    public int Value { get; set; }
}