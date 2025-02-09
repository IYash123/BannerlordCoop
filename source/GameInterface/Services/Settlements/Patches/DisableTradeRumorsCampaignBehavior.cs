﻿using HarmonyLib;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace GameInterface.Services.Settlements.Patches;


[HarmonyPatch(typeof(TradeRumorsCampaignBehavior))]
internal class DisableTradeRumorsCampaignBehavior
{
    [HarmonyPatch(nameof(TradeRumorsCampaignBehavior.RegisterEvents))]
    static bool Prefix() => false;
}
