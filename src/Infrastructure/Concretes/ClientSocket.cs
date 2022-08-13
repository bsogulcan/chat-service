using System.Net.Sockets;
using System.Text;
using Infrastructure.Modal;
using Newtonsoft.Json;

namespace Infrastructure.Concretes;

public class ClientSocket : SocketWrapper
{
    #region ManuelResetEvent properties for event management

    private readonly ManualResetEvent _connectDone;
    private readonly ManualResetEvent _sendDone;
    private readonly ManualResetEvent _receiveDone;

    #endregion

    public string LastResponseFromServer { get; set; }
    private readonly System.Timers.Timer _timer;

    private readonly MessageDto _messageDto;
    private readonly Guid _clientId;

    public ClientSocket()
    {
        _connectDone = new ManualResetEvent(false);
        _sendDone = new ManualResetEvent(false);
        _receiveDone = new ManualResetEvent(false);

        // Initialize Timer to try reconnect when connection unsuccessful
        _timer = new System.Timers.Timer()
        {
            Interval = 5000,
            Enabled = false,
        };
        _timer.Elapsed += (sender, args) => Connect();

        _messageDto = new MessageDto();
        _clientId = Guid.NewGuid();
    }

    public Guid GetClientId()
    {
        return _clientId;
    }

    /// <summary>
    /// Start Client Connection
    /// </summary>
    public override void Start(bool directly = true)
    {
        if (Socket == null)
        {
            throw new NullReferenceException("Socket should be initialized.");
        }

        // Starting connection process
        Connect();

        if (directly)
        {
            ListenConsoleInput();
        }
    }

    #region Private Methods

    private void Connect()
    {
        Socket.BeginConnect(IpEndPoint, ConnectCallback, Socket);

        // Waiting for connection is done
        _connectDone.WaitOne();
    }

    public void ListenConsoleInput()
    {
        while (true)
        {
            // Waiting input from console
            SendMessage(Console.ReadLine());
        }
    }

    public void SendMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        // Send input to the server
        Send(Socket, message);
        _sendDone.WaitOne();

        // Receive the response from the server  
        Receive(Socket);
        _receiveDone.WaitOne();
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket) ar.AsyncState;

            // Complete the connection.  
            client.EndConnect(ar);
            _timer.Enabled = false;

            Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint);

            // Signal that the connection has been made.  
            _connectDone.Set();
        }
        catch
        {
            // Start Timer try to start connection
            _timer.Enabled = true;
            Console.WriteLine("Server not responding for now. Retrying in 5 seconds.");
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

        var byteData = Encoding.ASCII.GetBytes(stringifyContent);

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
            Console.WriteLine("Your connection was canceled by the server.");
        }
    }

    private void Receive(Socket client)
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
            //Console.WriteLine(e.ToString());
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
                var response = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);
                //Console.WriteLine(response);
                LastResponseFromServer = response;

                _receiveDone.Set();

                if (response.Contains("Kicked"))
                {
                    Environment.Exit(0);
                }

                // Get the rest of the data.  
                client.BeginReceive(state.Buffer, 0, state.BufferSize, 0,
                    ReceiveCallback, state);
            }
        }
        catch (Exception e)
        {
            //Console.WriteLine(e.ToString());
        }
    }

    #endregion
}