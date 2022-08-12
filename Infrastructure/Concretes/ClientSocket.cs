using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using Infrastructure.Modal;
using Newtonsoft.Json;

namespace Infrastructure.Concretes;

public class ClientSocket : SocketWrapper
{
    // ManualResetEvent instances signal completion.  
    private readonly ManualResetEvent _connectDone = new ManualResetEvent(false);
    private readonly ManualResetEvent _sendDone = new ManualResetEvent(false);
    private readonly ManualResetEvent _receiveDone = new ManualResetEvent(false);
    private MessageDto _messageDto = new();
    private Guid _clientId = Guid.NewGuid();

    public override void Start()
    {
        Socket = new Socket(IpAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);
        // Connect to the remote endpoint.  
        Socket.BeginConnect(IpEndPoint,
            ConnectCallback, Socket);
        _connectDone.WaitOne();

        SendMessages();
    }

    private void SendMessages()
    {
        while (true)
        {
            var message = Console.ReadLine();
            // Send test data to the remote device.  
            Send(Socket, message);
            _sendDone.WaitOne();

            // Receive the response from the remote device.  
            Receive(Socket);
            _receiveDone.WaitOne();

            // Write the response to the console.  
        }
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket) ar.AsyncState;

            // Complete the connection.  
            client.EndConnect(ar);

            Console.WriteLine("Socket connected to {0}",
                client.RemoteEndPoint.ToString());

            // Signal that the connection has been made.  
            _connectDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }


    public override void Send(Socket handler, string data)
    {
        // Convert the string data to byte data using ASCII encoding.  

        var content = new MessageContent()
        {
            ClientId = _clientId,
            DateTime = DateTime.UtcNow,
            Message = data
        };
        var stringifyContent = JsonConvert.SerializeObject(content);

        byte[] byteData = Encoding.ASCII.GetBytes(stringifyContent);

        // Begin sending the data to the remote device.  
        Socket.BeginSend(byteData, 0, byteData.Length, 0,
            SendCallback, Socket);

        //Console.WriteLine("Sent {0} data to server.", stringifyContent);
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket) ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = client.EndSend(ar);
            //Console.WriteLine("Sent {0} bytes to server.", bytesSent);

            // Signal that all bytes have been sent.  
            _sendDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public void Receive(Socket client)
    {
        try
        {
            // Create the state object.  
            _messageDto.WorkSocket = client;
            // MessageDto state = new MessageDto
            // {
            //     WorkSocket = client
            // };

            // Begin receiving the data from the remote device.  
            client.BeginReceive(_messageDto.Buffer, 0, _messageDto.BufferSize, 0,
                ReceiveCallback, _messageDto);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the state object and the client socket
            // from the asynchronous state object.  
            MessageDto state = (MessageDto) ar.AsyncState;
            Socket client = state.WorkSocket;

            // Read data from the remote device.  
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.  
                var rawData = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);
                state.MessageContent = new MessageContent()
                {
                    Message = rawData
                };

                _receiveDone.Set();

                // Get the rest of the data.  
                client.BeginReceive(state.Buffer, 0, state.BufferSize, 0,
                    ReceiveCallback, state);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}