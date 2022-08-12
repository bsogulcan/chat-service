using Infrastructure.Concretes;

namespace Infrastructure.SocketFactory;

public class SocketFactory : ISocketFactory
{
    public ClientSocket CreateClient()
    {
        return new ClientSocket();
    }

    public ServerSocket CreateServer()
    {
        return new ServerSocket();
    }
}