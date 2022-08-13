using System.Threading;
using NUnit.Framework;

namespace Infrastructure.Test;

public class InfrastructureTest
{
    private SocketFactory.SocketFactory _socketFactory;

    [SetUp]
    public void Setup()
    {
        _socketFactory = new SocketFactory.SocketFactory();
    }

    [Test]
    public void Infrastructure_WhenClientSendingMessageSuccessFully_ServerShouldReturnOk()
    {
        var port = PortManager.GetNextUnusedPort(4567, 5000);

        var server = _socketFactory.CreateServer();
        server.Initialize(port);
        server.Start(false);

        var client = _socketFactory.CreateClient();
        client.Initialize(port);
        client.Start(false);

        client.SendMessage("Test");

        // I have to sleep thread one sec because of be sure to server send message back to client.
        Thread.Sleep(1000);

        Assert.AreEqual(client.LastResponseFromServer, "OK!");
    }

    [Test]
    public void Infrastructure_WhenClientSendingTwoMessagesSuccessFully_ServerSendWarningMessage()
    {
        var port = PortManager.GetNextUnusedPort(4567, 5000);

        var server = _socketFactory.CreateServer();
        server.Initialize(port);
        server.Start(false);

        var client = _socketFactory.CreateClient();
        client.Initialize(port);
        client.Start(false);

        client.SendMessage("First message");
        client.SendMessage("Second message");

        // I have to sleep thread one sec because of server processing that client is dangerous or not 
        // and sending message to client.
        Thread.Sleep(1000);
        Assert.AreEqual(client.LastResponseFromServer, "Warning! Sending too much Messages per seconds.");
    }
}