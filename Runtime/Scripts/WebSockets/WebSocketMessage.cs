using System;

[Serializable]
public class WebSocketMessage
{
    public string type;
    public string payload;

    public WebSocketMessage(string type, string payload)
    {
        this.type = type;
        this.payload = payload;
    }
}