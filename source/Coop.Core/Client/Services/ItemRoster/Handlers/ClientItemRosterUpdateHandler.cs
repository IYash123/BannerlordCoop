using Common.Logging;
using Common.Messaging;
using Common.Network;
using Coop.Core.Client.Services.ItemRosters.Messages;
using Coop.Core.Server.Services.ItemRoster.Messages;
using GameInterface.Services.ItemRosters.Messages;
using GameInterface.Services.Misc.Patches;
using Serilog;

namespace Coop.Core.Client.Services.ItemRoster.Handlers
{
    public class ClientItemRosterUpdateHandler : IHandler
    {
        private readonly IMessageBroker messageBroker;
        private readonly INetwork network;
        private readonly ILogger Logger = LogManager.GetLogger<ClientItemRosterUpdateHandler>();

        public ClientItemRosterUpdateHandler(IMessageBroker messageBroker, INetwork network, ILogger logger)
        {
            this.messageBroker = messageBroker;
            this.network = network;

            messageBroker.Subscribe<ItemRosterChangeAttempt>(Handle);
            messageBroker.Subscribe<NetworkItemRosterChange>(Handle);

        }

        public void Dispose()
        {
            messageBroker.Unsubscribe<ItemRosterChangeAttempt>(Handle);
        }


        private void Handle(MessagePayload<ItemRosterChangeAttempt> obj)
        {
            var payload = obj.What;

            var itemRosterChangeAttempt = new NetworkItemRosterChangeAttempt(payload.ItemId, payload.ItemModifierId, payload.Amount);

            network.SendAll(itemRosterChangeAttempt);
        }

        private void Handle(MessagePayload<NetworkItemRosterChange> obj)
        {
            var payload = obj.What;

            messageBroker.Publish(this, new ItemRosterChangeApproved(payload.ItemId, payload.ItemModifierId, payload.Amount));
        }
    }
}