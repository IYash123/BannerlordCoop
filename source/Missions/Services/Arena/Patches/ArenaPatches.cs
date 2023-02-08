﻿using Common.Network;
using Common.Serialization;
using GameInterface.Serialization;
using GameInterface.Serialization.External;
using HarmonyLib;
using Missions.Services.Agents.Packets;
using Missions.Services.Network;
using System;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace Missions.Services.Arena.Patches
{
    [HarmonyPatch(typeof(Mission), "RegisterBlow")]
    public class AgentDamagePatch
    {
        static bool Prefix(Agent attacker, Agent victim, GameEntity realHitEntity, Blow b, ref AttackCollisionData collisionData, in MissionWeapon attackerWeapon, ref CombatLogData combatLogData)
        {
            // first, check if the attacker exists in the agent to ID groud, if not, no networking is needed (not a network agent)
            if (!NetworkAgentRegistry.Instance.AgentToId.TryGetValue(attacker, out Guid attackerId)) return true;

            // next, check if the attacker is one of ours, if not, no networking is needed (not our agent dealing damage)
            if (!NetworkAgentRegistry.Instance.ControlledAgents.ContainsKey(attackerId)) return true;

            AgentDamageData _agentDamageData;

            // get the victim GUI
            NetworkAgentRegistry.Instance.AgentToId.TryGetValue(victim, out Guid victimId);

            // construct a agent damage data
            _agentDamageData = new AgentDamageData(attackerId, victimId, b.InflictedDamage, collisionData, b);

            // publish the event
            NetworkMessageBroker.Instance.PublishNetworkEvent(_agentDamageData);

            return true;
        }
    }
}