using Infrastructure.Concretes;

namespace Infrastructure.SocketFactory;

public interface ISocketFactory
{
    ClientSocket CreateClient();
    ServerSocket CreateServer();
}