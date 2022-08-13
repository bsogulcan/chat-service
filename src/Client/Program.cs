using Infrastructure.SocketFactory;

Console.WriteLine("Starting ChatService-Client");
var client = new SocketFactory().CreateClient();
client.Initialize();
client.Start();
client.ListenConsoleInput();