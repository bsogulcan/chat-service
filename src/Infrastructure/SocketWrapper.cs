using System.Net;
using System.Net.Sockets;

namespace Infrastructure;

public abstract class SocketWrapper
{
    protected Socket Socket { get; set; }
    private IPHostEntry IpHostEntry { get; set; }
    protected IPAddress IpAddress { get; set; }
    protected IPEndPoint IpEndPoint { get; set; }
    private int Port { get; set; } = 4567;

    public abstract void Start();
    public abstract void Send(Socket handler, String data);

    public void Initialize()
    {
        IpHostEntry = Dns.GetHostEntry(Dns.GetHostName());
        IpAddress = IpHostEntry.AddressList[0];
        IpEndPoint = new IPEndPoint(IpAddress, Port);
    }
}