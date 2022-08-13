using System;
using Infrastructure.SocketFactory;
using NUnit.Framework;

namespace Client.Test;

public class ClientTest
{
    private SocketFactory _socketFactory;

    [SetUp]
    public void Setup()
    {
        _socketFactory = new SocketFactory();
    }

    [Test]
    public void Client_WhenSocketNotStarted_ShouldBeFalse()
    {
        var client = _socketFactory.CreateClient();
        var clientStatus = client.IsConnectionStarted();

        Assert.AreEqual(clientStatus, false);
    }

    [Test]
    public void Server_WhenSocketNotInitialized_ShouldThrowEx()
    {
        var client = _socketFactory.CreateServer();
        Assert.Throws<NullReferenceException>(() => client.Start(false));
    }
}