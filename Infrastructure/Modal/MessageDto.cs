using System.Net.Sockets;

namespace Infrastructure.Modal;

public class MessageDto
{
    public MessageContent MessageContent { get; set; }
    public Socket WorkSocket { get; set; }
    public int BufferSize = 1024;
    public byte[] Buffer = new byte[1024];
}

public class MessageContent
{
    public Guid ClientId { get; set; }
    public DateTime DateTime { get; set; }
    public string Message { get; set; }
    
    public MessageContent()
    {
        ClientId = Guid.NewGuid();
    }
}