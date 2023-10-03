using Common.Messaging;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameInterface.Services.ItemRosters.Messages
{
    /// <summary>
    /// Triggered when an Item Roster tries to change
    /// </summary>
    public record ItemRosterChangeAttempt : IEvent
    {
        public string ItemId { get; }
        public string ItemModifierId { get; }
        public int Amount { get; }

        public ItemRosterChangeAttempt(string itemId, string itemModifierId, int amount)
        {
            ItemId = itemId;
            ItemModifierId = itemModifierId;
            Amount = amount;
        }
    }
}
