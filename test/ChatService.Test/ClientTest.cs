using Infrastructure.SocketFactory;
using NUnit.Framework;

namespace ChatService.Test;

public class ClientTest
{
    private SocketFactory _socketFactory;

    [SetUp]
    public void Setup()
    {
        _socketFactory = new SocketFactory();
    }

    [Test]
    public void Client_WhenClientSendingMessageSuccessFully_ServerShouldReturnOk()
    {
        var server = _socketFactory.CreateServer();
        server.Initialize();
        server.Start(false);

        var client = _socketFactory.CreateClient();
        client.Initialize();
        client.Start(false);

        client.SendMessage("Test");

        Assert.AreEqual(client.LastResponseFromServer, "OK!");
    }
}