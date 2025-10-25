namespace RiderHotReloadTest;
public interface IMessageService
{ 
    string GetMessage();
}

public class MessageService : IMessageService
{
    public string GetMessage()
    {
        return "This message comes from a separate service!";
    }
}