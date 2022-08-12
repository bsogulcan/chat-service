using Infrastructure.SocketFactory;

Console.WriteLine("Starting ChatService - Server");

var server = new SocketFactory().CreateServer();
server.Initialize();
server.Start();