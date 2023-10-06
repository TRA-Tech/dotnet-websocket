namespace TraTech.WebSocketHub.Abstractions
{
    /// <summary>
    /// Defines the interface for handling WebSocket requests.
    /// </summary>
    /// <remarks>
    /// This interface defines a method for handling WebSocket requests with the specified key and data.
    /// </remarks>
    public interface IWebSocketRequestHandler
    {
        /// <summary>
        /// Handles the WebSocket request with the specified key and data.
        /// </summary>
        /// <param name="key">The key associated with the WebSocket connection.</param>
        /// <param name="data">The data received from the WebSocket connection.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task HandleRequestAsync(string key, string data);
    }
}
