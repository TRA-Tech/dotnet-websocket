using System.Diagnostics.CodeAnalysis;

namespace TraTech.WebSocketHub
{
    public interface IWebSocketRequestHandlerProvider
    {
        public Task<Type?> GetHandlerAsync(string messageType);
        public void AddHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(string messageType) where THandler : class, IWebSocketRequestHandler;
        public bool TryAddHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(string messageType) where THandler : class, IWebSocketRequestHandler;
    }
}
