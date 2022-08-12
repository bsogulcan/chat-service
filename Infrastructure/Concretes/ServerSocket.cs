﻿using System.Net.Sockets;
using System.Text;
using Infrastructure.Modal;
using Newtonsoft.Json;

namespace Infrastructure.Concretes;

public class ServerSocket : SocketWrapper
{
    private readonly ManualResetEvent _allDone = new(false);

    public override void Start()
    {
        Socket = new Socket(IpAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        Socket.Bind(IpEndPoint);
        Socket.Listen(1000);
        WaitForClients();
    }

    private void WaitForClients()
    {
        Console.WriteLine("Waiting for a connection...");

        while (true)
        {
            // Set the event to nonsignaled state.  
            _allDone.Reset();

            // Start an asynchronous socket to listen for connections.  
            Socket.BeginAccept(
                AcceptCallback,
                Socket);

            // Wait until a connection is made before continuing.  
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
            int bytesSent = handler.EndSend(ar);
            //Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            //handler.Shutdown(SocketShutdown.Both);
            //handler.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private void Receive(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        MessageDto state = (MessageDto) ar.AsyncState;
        Socket handler = state.WorkSocket;

        // Read data from the client socket.
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.  
            var rawData = Encoding.ASCII.GetString(
                state.Buffer, 0, bytesRead);
            state.MessageContent = JsonConvert.DeserializeObject<MessageContent>(rawData);

            // Check for end-of-file tag. If it is not there, read
            // more data.  
            // All the data has been read from the
            // client. Display it on the console.  
            Console.WriteLine("Read data from socket : {0}", rawData);
            // Echo the data back to the client.  
            Send(handler, state.MessageContent.Message);

            handler.BeginReceive(state.Buffer, 0, state.BufferSize, 0,
                Receive, state);
        }
    }
}