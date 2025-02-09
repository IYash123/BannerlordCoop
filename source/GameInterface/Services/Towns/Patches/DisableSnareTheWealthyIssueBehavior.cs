﻿using HarmonyLib;
using SandBox.Issues;
using TaleWorlds.CampaignSystem.Issues;

namespace GameInterface.Services.Towns.Patches;

[HarmonyPatch(typeof(SnareTheWealthyIssueBehavior))]
internal class DisableSnareTheWealthyIssueBehavior
{
    [HarmonyPatch(nameof(SnareTheWealthyIssueBehavior.RegisterEvents))]
    static bool Prefix() => false;
}
