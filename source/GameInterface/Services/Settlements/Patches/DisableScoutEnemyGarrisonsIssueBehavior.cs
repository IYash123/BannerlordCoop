﻿using HarmonyLib;
using TaleWorlds.CampaignSystem.Issues;

namespace GameInterface.Services.Settlements.Patches;


[HarmonyPatch(typeof(ScoutEnemyGarrisonsIssueBehavior))]
internal class DisableScoutEnemyGarrisonsIssueBehavior
{
    [HarmonyPatch(nameof(ScoutEnemyGarrisonsIssueBehavior.RegisterEvents))]
    static bool Prefix() => false;
}
