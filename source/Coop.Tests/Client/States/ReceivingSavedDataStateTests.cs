﻿using Coop.Core.Client;
using Coop.Core.Client.Messages;
using Coop.Core.Client.States;
using GameInterface.Services.GameState.Messages;
using GameInterface.Services.Modules.Messages;
using Moq;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Coop.Tests.Client.States
{
    public class ReceivingSavedDataStateTests : CoopTest
    {
        private readonly IClientLogic clientLogic;
        public ReceivingSavedDataStateTests(ITestOutputHelper output) : base(output)
        {
            var mockCoopClient = new Mock<ICoopClient>();
            clientLogic = new ClientLogic(mockCoopClient.Object, NetworkMessageBroker);
            clientLogic.State = new ReceivingSavedDataState(clientLogic);
        }

        [Fact]
        public void Dispose_RemovesAllHandlers()
        {
            Assert.NotEqual(0, MessageBroker.GetTotalSubscribers());

            clientLogic.State.Dispose();

            Assert.Equal(0, MessageBroker.GetTotalSubscribers());
        }

        [Fact]
        public void NetworkGameSaveDataRecieved_Publishes_EnterMainMenuEvent()
        {
            var isEventPublished = false;
            MessageBroker.Subscribe<EnterMainMenu>((payload) =>
            {
                isEventPublished = true;
            });

            NetworkMessageBroker.ReceiveNetworkEvent(null, new NetworkGameSaveDataRecieved());

            Assert.True(isEventPublished);
        }

        [Fact]
        public void MainMenuEntered_Publishes_LoadGameSave()
        {
            var isEventPublished = false;
            MessageBroker.Subscribe<LoadGameSave>((payload) =>
            {
                isEventPublished = true;
            });

            NetworkMessageBroker.ReceiveNetworkEvent(null, new NetworkGameSaveDataRecieved(new byte[] { 1 }));
            MessageBroker.Publish(this, new MainMenuEntered());

            Assert.True(isEventPublished);
        }

        [Fact]
        public void MainMenuEntered_Transitions_LoadingState()
        {
            NetworkMessageBroker.ReceiveNetworkEvent(null, new NetworkGameSaveDataRecieved(new byte[] { 1 }));
            MessageBroker.Publish(this, new MainMenuEntered());

            Assert.IsType<LoadingState>(clientLogic.State);
        }

        [Fact]
        public void MainMenuEntered_Handles_NullData()
        {
            var isEventPublished = false;
            MessageBroker.Subscribe<LoadGameSave>((payload) =>
            {
                isEventPublished = true;
            });

            NetworkMessageBroker.ReceiveNetworkEvent(null, new NetworkGameSaveDataRecieved(null));
            MessageBroker.Publish(this, new MainMenuEntered());

            Assert.IsType<ReceivingSavedDataState>(clientLogic.State);
            Assert.False(isEventPublished);
        }

        [Fact]
        public void MainMenuEntered_Handles_ZeroLenArray()
        {
            var isEventPublished = false;
            MessageBroker.Subscribe<LoadGameSave>((payload) =>
            {
                isEventPublished = true;
            });

            NetworkMessageBroker.ReceiveNetworkEvent(null, new NetworkGameSaveDataRecieved(Array.Empty<byte>()));
            MessageBroker.Publish(this, new MainMenuEntered());

            Assert.IsType<ReceivingSavedDataState>(clientLogic.State);
            Assert.False(isEventPublished);
        }

        [Fact]
        public void EnterMainMenu_Publishes_EnterMainMenuEvent()
        {
            var isEventPublished = false;
            MessageBroker.Subscribe<EnterMainMenu>((payload) =>
            {
                isEventPublished = true;
            });

            clientLogic.EnterMainMenu();

            Assert.True(isEventPublished);
        }

        [Fact]
        public void Disconnect_Publishes_EnterMainMenu()
        {
            var isEventPublished = false;
            MessageBroker.Subscribe<EnterMainMenu>((payload) =>
            {
                isEventPublished = true;
            });

            clientLogic.Disconnect();

            Assert.True(isEventPublished);
        }

        [Fact]
        public void Disconnect_Transitions_EnterMainMenu()
        {
            clientLogic.Disconnect();

            Assert.IsType<MainMenuState>(clientLogic.State);
        }

        [Fact]
        public void OtherStateMethods_DoNotAlterState()
        {
            clientLogic.Connect();
            Assert.IsType<ReceivingSavedDataState>(clientLogic.State);

            clientLogic.ExitGame();
            Assert.IsType<ReceivingSavedDataState>(clientLogic.State);

            clientLogic.LoadSavedData();
            Assert.IsType<ReceivingSavedDataState>(clientLogic.State);

            clientLogic.StartCharacterCreation();
            Assert.IsType<ReceivingSavedDataState>(clientLogic.State);

            clientLogic.EnterCampaignState();
            Assert.IsType<ReceivingSavedDataState>(clientLogic.State);

            clientLogic.EnterMissionState();
            Assert.IsType<ReceivingSavedDataState>(clientLogic.State);

            clientLogic.ResolveNetworkGuids();
            Assert.IsType<ReceivingSavedDataState>(clientLogic.State);

            clientLogic.ValidateModules();
            Assert.IsType<ReceivingSavedDataState>(clientLogic.State);
        }
    }
}