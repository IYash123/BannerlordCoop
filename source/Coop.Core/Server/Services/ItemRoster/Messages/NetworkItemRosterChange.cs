using Common.Messaging;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Coop.Core.Server.Services.ItemRoster.Messages
{
    /// <summary>
    /// Message from the server about an itemRosterChange
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public record NetworkItemRosterChange : ICommand
    {
        [ProtoMember(1)]
        public string ItemId { get; }
        [ProtoMember(2)]
        public string ItemModifierId { get; }
        [ProtoMember(3)]
        public int Amount { get; }

        public NetworkItemRosterChange(string itemId, string itemModifierId, int amount)
        {
            ItemId = itemId;
            ItemModifierId = itemModifierId;
            Amount = amount;
        }
    }
}
