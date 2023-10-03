using Common.Logging;
using Common.Messaging;
using Common.Network;
using Coop.Core.Client.Services.ItemRoster;
using Coop.Core.Client.Services.MobileParties.Messages;
using Coop.Core.Server.Services.ItemRoster.Messages;
using Coop.Core.Server.Services.MobileParties.Handlers;
using GameInterface.Services.Misc.Patches;
using GameInterface.Services.MobileParties.Messages.Behavior;
using LiteNetLib;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Coop.Core.Server.Services.ItemRoster.Handlers
{
    public class ServerItemRosterUpdateHandler : IHandler
    {
        private readonly IMessageBroker messageBroker;
        private readonly INetwork network;
        private readonly ILogger Logger = LogManager.GetLogger<ServerItemRosterUpdateHandler>();

        public ServerItemRosterUpdateHandler(IMessageBroker messageBroker, INetwork network, ILogger logger)
        {
            this.messageBroker = messageBroker;
            this.network = network;

            messageBroker.Subscribe<NetworkItemRosterChangeAttempt>(Handle);


        }
        public void Dispose()
        {
            messageBroker.Unsubscribe<NetworkItemRosterChangeAttempt>(Handle);
        }


        private void Handle(MessagePayload<NetworkItemRosterChangeAttempt> obj)
        {
            var payload = obj.What;

            var itemRosterChangeAttempt = new NetworkItemRosterChange(payload.ItemId, payload.ItemModifierId, payload.Amount);

            network.SendAll(itemRosterChangeAttempt);
        }




    }
}
