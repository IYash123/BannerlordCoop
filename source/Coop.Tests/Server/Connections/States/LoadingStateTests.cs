﻿using Coop.Core.Server.Connections.States;
using Xunit;
using Xunit.Abstractions;

namespace Coop.Tests.Server.Connections.States
{
    public class LoadingStateTests : CoopTest
    {
        private readonly IConnectionLogic _connectionLogic;

        public LoadingStateTests(ITestOutputHelper output) : base(output)
        {
            _connectionLogic = new ConnectionLogic(messageBroker);
        }

        [Fact]
        public void EnterCampaignMethod_TransitionState_CampaignState()
        {
            _connectionLogic.State = new LoadingState(_connectionLogic, messageBroker);

            _connectionLogic.EnterCampaign();

            Assert.IsType<CampaignState>(_connectionLogic.State);
        }

        [Fact]
        public void UnusedStatesMethods_DoNothing()
        {
            _connectionLogic.State = new ResolveCharacterState(_connectionLogic, messageBroker);

            _connectionLogic.ResolveCharacter();
            _connectionLogic.Load();
            _connectionLogic.EnterMission();

            Assert.IsType<LoadingState>(_connectionLogic.State);
        }
    }
}