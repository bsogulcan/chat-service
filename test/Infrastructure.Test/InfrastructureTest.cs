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
    public void Infrastructure_WhenClientSendingMessage_ShouldServerReturnOk()
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
        Thread.Sleep(500);

        Assert.AreEqual(client.LastResponseFromServer, "OK!");
    }

    [Test]
    public void Infrastructure_WhenClientSendingTwoMessages_ShouldServerSendWarningMessage()
    {
        //var port = PortManager.GetNextUnusedPort(4567, 5000);
        var server = _socketFactory.CreateServer();
        server.Initialize(4666);
        server.Start(false);

        var client = _socketFactory.CreateClient();
        client.Initialize(4666);
        client.Start(false);

        client.SendMessage("First message");
        client.SendMessage("Second message");

        // I have to sleep thread one sec because of server processing that client is dangerous or not 
        // and sending message to client.
        Thread.Sleep(1000);
        Assert.AreEqual(client.LastResponseFromServer, "Warning! Sending too much Messages per seconds.");
    }

    [Test]
    public void Infrastructure_WhenClientSendingTwoMessages_ShouldServerChangeClientsStatusToDangerous()
    {
        //var port = PortManager.GetNextUnusedPort(4567, 5000);
        var server = _socketFactory.CreateServer();
        server.Initialize(4777);
        server.Start(false);

        var client = _socketFactory.CreateClient();
        client.Initialize(4777);
        client.Start(false);

        var clientId = client.GetClientId();

        client.SendMessage("First message");
        client.SendMessage("Second message");

        // I have to sleep thread one sec because of server processing that client is dangerous or not 
        // and sending message to client.
        Thread.Sleep(1000);

        var clientStatus = server.GetClientStatus(clientId);

        Assert.AreEqual(clientStatus.IsDangerous(), true);
    }

    [Test]
    public void Infrastructure_WhenClientSendingThreeMessagesInsteadOfWarning_ShouldServerDisconnectWithClient()
    {
        //var port = PortManager.GetNextUnusedPort(4567, 5000);
        var server = _socketFactory.CreateServer();
        server.Initialize(4888);
        server.Start(false);

        var client = _socketFactory.CreateClient();
        client.Initialize(4888);
        client.Start(false);

        client.SendMessage("First message");
        client.SendMessage("Second message");
        client.SendMessage("I'll send more messages!");

        // I have to sleep thread one sec because of server processing that client is dangerous or not 
        // and sending message to client.
        Thread.Sleep(1000);

        Assert.AreEqual(client.LastResponseFromServer,
            "You were kicked out of the server for making too many requests.");
    }

    [Test]
    public void Infrastructure_WhenClientSendingMessageAfterWarningMessage_ShouldServerChangedDangerousStatusToFalse()
    {
        //var port = PortManager.GetNextUnusedPort(4567, 5000);
        var server = _socketFactory.CreateServer();
        server.Initialize(5100);
        server.Start(false);

        var client = _socketFactory.CreateClient();
        client.Initialize(5100);
        client.Start(false);

        client.SendMessage("First message");
        client.SendMessage("Second message");

        // Waiting for the warning.
        Thread.Sleep(1300);
        client.SendMessage("I'll send more messages!");

        // I have to sleep thread one sec because of server processing that client is dangerous or not 
        // and sending message to client.
        Thread.Sleep(1000);

        Assert.AreEqual(client.LastResponseFromServer, "OK!");
    }
}