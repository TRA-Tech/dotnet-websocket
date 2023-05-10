using System.Diagnostics.CodeAnalysis;

namespace TraTech.WebSocketHub
{
    public class WebSocketRequestHandlerProvider : IWebSocketRequestHandlerProvider
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, Type> _handlerTypeMap = new(StringComparer.Ordinal);

        public bool TryAddHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(string messageType)
            where THandler : class, IWebSocketRequestHandler
        {
            if (_handlerTypeMap.ContainsKey(messageType))
            {
                return false;
            }
            if (!typeof(IWebSocketRequestHandler).IsAssignableFrom(typeof(THandler)))
            {
                throw new InvalidOperationException($"{typeof(THandler)} is not assignable from IWebSocketRequestHandler");
            }

            lock (_lock)
            {
                _handlerTypeMap.Add(messageType, typeof(THandler));
                return true;
            }
        }

        public void AddHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(string messageType)
            where THandler : class, IWebSocketRequestHandler
        {
            if (_handlerTypeMap.ContainsKey(messageType))
            {
                throw new InvalidOperationException("WebSocketRequestHandler already exists: " + messageType);
            }
            lock (_lock)
            {
                if (!TryAddHandler<THandler>(messageType))
                {
                    throw new InvalidOperationException("WebSocketRequestHandler already exists: " + messageType);
                }
            }
        }

        public Task<Type?> GetHandlerAsync(string messageType)
        {
            return Task.FromResult(_handlerTypeMap.TryGetValue(messageType, out var handlerType) ? handlerType : null);
        }
    }
}
