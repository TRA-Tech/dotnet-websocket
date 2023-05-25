namespace TraTech.WebSocketHub
{
    /// <summary>
    /// Represents a message sent over a WebSocket connection.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payload of the message.
        /// </summary>
        public object? Payload { get; set; }
    }
}
