namespace ExampleWebApplication.WebSocketHubKeys
{
    public class SocketUser
    {
        public int Id { get; private set; }
        public DateTime OpenDate { get; private set; }

        public SocketUser(int id)
        {
            Id = id;
            OpenDate = DateTime.UtcNow;
        }
    }
}
