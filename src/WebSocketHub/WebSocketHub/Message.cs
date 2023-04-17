namespace TraTech.WebSocketHub
{
    public class Message
    {
        public string Type { get; set; } = string.Empty;
        public object? Payload { get; set; }
    }
}
