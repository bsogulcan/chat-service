using System;
using Infrastructure.Concretes;
using Infrastructure.SocketFactory;
using NUnit.Framework;

namespace ChatService.Test;

public class ServerTest
{
    private SocketFactory _socketFactory;

    [SetUp]
    public void Setup()
    {
        _socketFactory = new SocketFactory();
    }

    [Test]
    public void SocketFactory_WhenSocketNotStarted_ShouldBeFalse()
    {
        var server = _socketFactory.CreateServer();
        var serverStatus = server.IsConnectionStarted();

        Assert.AreEqual(serverStatus, false);
    }

    [Test]
    public void SocketFactory_WhenSocketStarted_ShouldBeListeningForClients()
    {
        var server = _socketFactory.CreateServer();
        server.Initialize();
        server.Start(false);
        var serverStatus = server.GetIsListening();

        Assert.AreEqual(serverStatus, 1);
    }

    [Test]
    public void SocketFactory_WhenSocketNotInitialized_ShouldThrowEx()
    {
        var server = _socketFactory.CreateServer();
        Assert.Throws<NullReferenceException>(() => server.Start(false));
    }


}