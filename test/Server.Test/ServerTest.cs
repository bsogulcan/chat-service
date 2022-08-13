using System;
using Infrastructure;
using Infrastructure.SocketFactory;
using NUnit.Framework;

namespace Server;

public class ServerTest
{
    private SocketFactory _socketFactory;

    [SetUp]
    public void Setup()
    {
        _socketFactory = new SocketFactory();
    }

    [Test]
    public void Server_WhenSocketNotStarted_ShouldBeFalse()
    {
        var server = _socketFactory.CreateServer();
        var serverStatus = server.IsConnectionStarted();

        Assert.AreEqual(serverStatus, false);
    }

    [Test]
    public void Server_WhenSocketStarted_ShouldBeListeningForClients()
    {
        //var port = PortManager.GetNextUnusedPort(4567, 5000);

        var server = _socketFactory.CreateServer();
        server.Initialize(4999);
        server.Start(false);
        var serverStatus = server.GetIsListening();

        Assert.AreEqual(serverStatus, 1);
    }

    [Test]
    public void Server_WhenSocketNotInitialized_ShouldThrowEx()
    {
        var server = _socketFactory.CreateServer();
        Assert.Throws<NullReferenceException>(() => server.Start(false));
    }
}