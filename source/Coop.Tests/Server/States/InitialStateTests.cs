﻿using GameInterface.Services.GameDebug.Messages;
using Moq;
using Xunit.Abstractions;
using Xunit;
using Coop.Core.Server.States;
using GameInterface.Services.GameState.Messages;
using Coop.Core.Server;

namespace Coop.Tests.Server.States
{
    public class InitialStateTests : CoopTest
    {
        public InitialStateTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void InitialStateStart()
        {
            Mock<IServerLogic> serverLogic = new Mock<IServerLogic>();
            Mock<ICoopServer> coopServer = new Mock<ICoopServer>();
            IServerState currentState = new InitialServerState(serverLogic.Object, MessageBroker);
            serverLogic.SetupSet(x => x.State = It.IsAny<IServerState>()).Callback<IServerState>(value => currentState = value);
            serverLogic.Setup(m => m.NetworkServer).Returns(coopServer.Object);

            MessageBroker.Subscribe<LoadDebugGame>((payload) =>
            {
                MessageBroker.Publish(null, new GameLoaded());
            });

            currentState.Start();

            Assert.IsType<ServerRunningState>(currentState);
        }

        [Fact]
        public void InitialStateStop()
        {

            Mock<IServerLogic> serverLogic = new Mock<IServerLogic>();
            IServerState currentState = new InitialServerState(serverLogic.Object, MessageBroker);
            serverLogic.SetupSet(x => x.State = It.IsAny<IServerState>()).Callback<IServerState>(value => currentState = value);

            currentState.Stop();

            Assert.IsType<InitialServerState>(currentState);
        }

        [Fact]
        public void InitialStateDispose()
        {

            Mock<IServerLogic> serverLogic = new Mock<IServerLogic>();
            IServerState currentState = new InitialServerState(serverLogic.Object, MessageBroker);

            Assert.NotEqual(0, MessageBroker.GetTotalSubscribers());

            currentState.Dispose();

            Assert.Equal(0, MessageBroker.GetTotalSubscribers());
        }
    }
}