using System.Diagnostics.CodeAnalysis;

namespace TraTech.WebSocketHub
{
    /// <summary>
    /// Defines the interface for providing WebSocket request handlers.
    /// </summary>
    /// <remarks>
    /// This interface defines methods for getting and adding WebSocket request handlers for the specified message types.
    /// </remarks>
    public interface IWebSocketRequestHandlerProvider
    {
        /// <summary>
        /// Gets the type of WebSocket request handler for the specified message type.
        /// </summary>
        /// <param name="messageType">The message type associated with the WebSocket request handler.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the type of WebSocket request handler for the specified message type, or null if no handler was found.</returns>
        Task<Type?> GetHandlerAsync(string messageType);

        /// <summary>
        /// Adds a WebSocket request handler of type THandler for the specified message type.
        /// </summary>
        /// <typeparam name="THandler">The type of WebSocket request handler to add.</typeparam>
        /// <param name="messageType">The message type associated with the WebSocket request handler.</param>
        /// <remarks>
        /// This method adds a WebSocket request handler of type THandler for the specified message type. The THandler type must implement the IWebSocketRequestHandler interface and have a public constructor.
        /// </remarks>
        void AddHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(string messageType) where THandler : class, IWebSocketRequestHandler;

        /// <summary>
        /// Tries to add a WebSocket request handler of type THandler for the specified message type.
        /// </summary>
        /// <typeparam name="THandler">The type of WebSocket request handler to add.</typeparam>
        /// <param name="messageType">The message type associated with the WebSocket request handler.</param>
        /// <returns>A boolean value indicating whether the WebSocket request handler was added successfully.</returns>
        /// <remarks>
        /// This method tries to add a WebSocket request handler of type THandler for the specified message type. The THandler type must implement the IWebSocketRequestHandler interface and have a public constructor. If a handler already exists for the specified message type, this method returns false and does not add the new handler.
        /// </remarks>
        bool TryAddHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(string messageType) where THandler : class, IWebSocketRequestHandler;
    }
}
