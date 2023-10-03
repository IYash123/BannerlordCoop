using Common.Messaging;

namespace Coop.Core.Client.Services.ItemRosters.Messages
{
    /// <summary>
    /// Triggered when an Item Roster tries to change
    /// </summary>
    public record ItemRosterChangeApproved : IEvent
    {
        public string ItemId { get; }
        public string ItemModifierId { get; }
        public int Amount { get; }

        public ItemRosterChangeApproved(string itemId, string itemModifierId, int amount)
        {
            ItemId = itemId;
            ItemModifierId = itemModifierId;
            Amount = amount;
        }
    }
}