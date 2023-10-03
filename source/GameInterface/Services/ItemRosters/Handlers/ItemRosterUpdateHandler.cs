using Common.Logging;
using Common.Messaging;
using Common.Network;
using Coop.Core.Client.Services.ItemRosters.Messages;
using GameInterface.Services.ItemRosters.Patches;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace GameInterface.Services.ItemRosters.Handlers
{
    public class ItemRosterUpdateHandler : IHandler
    {
        private readonly IMessageBroker messageBroker;
        private readonly INetwork network;
        private readonly ILogger Logger = LogManager.GetLogger<ItemRosterUpdateHandler>();

        public ItemRosterUpdateHandler(IMessageBroker messageBroker, INetwork network, ILogger logger)
        {
            this.messageBroker = messageBroker;
            this.network = network;

            messageBroker.Subscribe<ItemRosterChangeApproved>(Handle);

        }

        public void Dispose()
        {
            messageBroker.Unsubscribe<ItemRosterChangeApproved>(Handle);
        }


        private void Handle(MessagePayload<ItemRosterChangeApproved> obj)
        {
            var payload = obj.What;

            ItemObject item = new ItemObject(payload.ItemId);

            ItemModifier itemModifier = MBObjectManager.Instance.GetObject<ItemModifier>(payload.ItemModifierId);

            ItemRosterPatch.OverrideAddToCounts(new EquipmentElement(item, itemModifier), payload.Amount);
        }
    }
}
