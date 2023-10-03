using Common.Messaging;
using ProtoBuf;

namespace GameInterface.Services.Misc.Patches
{
    /// <summary>
    /// Triggered when an Item Roster tries to change
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public record NetworkItemRosterChangeAttempt : ICommand
    {
        [ProtoMember(1)]
        public string ItemId { get; }
        [ProtoMember(2)]
        public string ItemModifierId { get; }
        [ProtoMember(3)]
        public int Amount { get; }

        public NetworkItemRosterChangeAttempt(string itemId, string itemModifierId, int amount)
        {
            ItemId = itemId;
            ItemModifierId = itemModifierId;
            Amount = amount;
        }
    }
}