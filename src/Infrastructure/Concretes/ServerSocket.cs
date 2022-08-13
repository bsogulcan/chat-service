using System.Net.Sockets;
using System.Text;
using Infrastructure.Modal;
using Newtonsoft.Json;

namespace Infrastructure.Concretes;

public class ServerSocket : SocketWrapper
{
    #region ManuelResetEvent properties for event management

    private readonly ManualResetEvent _allDone;

    #endregion

    private readonly List<MessageContent> _messageHistory;
    private readonly List<ClientStatus> _clientStatus;

    public object GetIsListening()
    {
        // 0 is not listening any client to connect. 1 is listening.
        return Socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.AcceptConnection);
    }

    public ServerSocket()
    {
        _allDone = new ManualResetEvent(false);
        _messageHistory = new List<MessageContent>();
        _clientStatus = new List<ClientStatus>();
    }

    public override void Start(bool directly = true)
    {
        if (Socket == null)
        {
            throw new NullReferenceException("Socket should be initialized.");
        }

        Socket.Bind(IpEndPoint);
        Socket.Listen(1000);

        WaitForClients(directly);
    }

    public ClientStatus GetClientStatus(Guid clientId)
    {
        var clientStatus = _clientStatus.FirstOrDefault(x => x.ClientId == clientId);

        if (clientStatus != null) return clientStatus;

        clientStatus = new ClientStatus(clientId);
        _clientStatus.Add(clientStatus);

        return clientStatus;
    }

    #region Private Methods

    private void WaitForClients(bool repeat = true)
    {
        Console.WriteLine("Waiting for a connection...");

        while (true)
        {
            // Set the event to non signaled state.  
            _allDone.Reset();

            // Start an asynchronous socket to listen for connections.  
            Socket.BeginAccept(AcceptCallback, Socket);

            // Wait until a connection is made before continuing.  

            if (!repeat)
            {
                break;
            }

            _allDone.WaitOne();
        }
    }

    private void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.  
        _allDone.Set();

        Console.WriteLine("Client Connected!");

        // Get the socket that handles the client request.  
        Socket listener = (Socket) ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        // Create the state object.  
        var messageDto = new MessageDto
        {
            WorkSocket = handler
        };

        handler.BeginReceive(messageDto.Buffer, 0, messageDto.BufferSize, 0,
            Receive, messageDto);
    }

    public override void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            SendCallback, handler);
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket handler = (Socket) ar.AsyncState;

            // Complete sending the data to the remote device.  
            handler.EndSend(ar);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private void Receive(IAsyncResult ar)
    {
        try
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            MessageDto state = (MessageDto) ar.AsyncState;
            Socket handler = state.WorkSocket;

            // Read data from the client socket.
            var bytesRead = handler.EndReceive(ar);

            if (bytesRead <= 0) return;

            // Read bytes from the server
            var rawData = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);

            state.Content = JsonConvert.DeserializeObject<MessageContent>(rawData);
            Console.WriteLine(
                $"Data Received From: {state.Content.ClientId}\nTime: {state.Content.DateTime} \nMessage: {state.Content.Message}");

            // Checking clients messages for Dangerous requests
            CheckClientHistory(handler, state);

            // Taking message log for each client
            _messageHistory.Add(state.Content);

            if (handler.Connected)
            {
                handler.BeginReceive(state.Buffer, 0, state.BufferSize, 0, Receive, state);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("Client disconnected from server.");
        }
    }

    private void CheckClientHistory(Socket handler, MessageDto messageDto)
    {
        var clientStatus = GetClientStatus(messageDto.Content.ClientId);

        var recentMessage = _messageHistory.Where(x => x.ClientId == messageDto.Content.ClientId)
            .OrderByDescending(x => x.DateTime).FirstOrDefault();
        if (recentMessage != null)
        {
            TimeSpan ts = messageDto.Content.DateTime - recentMessage.DateTime;
            var differenceSecondWithLastMessage = ts.TotalSeconds;
            if (differenceSecondWithLastMessage <= 1)
            {
                if (!clientStatus.IsDangerous())
                {
                    // Sending Warning message to client  
                    Send(handler, "Warning! Sending too much Messages per seconds.");
                    Console.WriteLine($"Send warning message to '{messageDto.Content.ClientId}' Client");
                    clientStatus.SetDangerous();
                }
                else
                {
                    // Shutdown current client connection
                    Console.WriteLine(
                        $"Shutdown '{messageDto.Content.ClientId}' client connection cause of sending too much messages against warning.");

                    Send(handler, "You were kicked out of the server for making too many requests.");

                    // Remove current client
                    _clientStatus.Remove(clientStatus);

                    // Dispose connection
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            else
            {
                clientStatus.SetDangerous(false);
                Send(handler, "OK!");
            }
        }
        else
        {
            Send(handler, "OK!");
        }
    }

    #endregion
}