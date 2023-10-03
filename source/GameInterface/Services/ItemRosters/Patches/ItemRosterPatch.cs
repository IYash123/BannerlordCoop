using Common.Messaging;
using Common.Util;
using GameInterface.Services.ItemRosters.Messages;
using HarmonyLib;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace GameInterface.Services.ItemRosters.Patches
{
    [HarmonyPatch(typeof(ItemRoster))]
    internal class ItemRosterPatch
    {
        public static readonly AllowedInstance<ItemObject> AllowedInstance = new AllowedInstance<ItemObject>();
        public static ItemRoster itemRoster;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemRoster.AddToCounts))]
        private static bool Prefix(ref ItemRoster __instance, EquipmentElement rosterElement, int number)
        {
            if (AllowedInstance.IsAllowed(rosterElement.Item)) return true;

            itemRoster = __instance;

            var message = new ItemRosterChangeAttempt(rosterElement.Item.StringId, rosterElement.ItemModifier?.StringId, number);

            MessageBroker.Instance.Publish(rosterElement, message);

            return false;
        }

        public static void OverrideAddToCounts(EquipmentElement rosterElement, int number)
        {
            using (AllowedInstance)
            {
                AllowedInstance.Instance = rosterElement.Item;

                if (rosterElement.Item is null) return;

                itemRoster.AddToCounts(rosterElement, number);
            }
        }
    }
}
