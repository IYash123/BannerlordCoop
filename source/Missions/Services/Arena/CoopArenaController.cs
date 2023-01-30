﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Common.Messaging;
using Common;
using Missions.Messages;
using LiteNetLib;
using Serilog;
using Common.Logging;
using Missions.Services.Network;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Encounters;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using TaleWorlds.CampaignSystem.Extensions;
using System.Runtime.CompilerServices;
using Missions.Services.Arena;
using TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;
using SandBox.View.Missions;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.CampaignSystem.Party;
using System.Reflection;

namespace Missions.Services
{
    internal class CoopArenaController : MissionBehavior
    {
        private static readonly ILogger Logger = LogManager.GetLogger<CoopArenaController>();

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        private readonly IMessageBroker _messageBroker;
        private readonly INetworkAgentRegistry _agentRegistry;
        private readonly IRandomEquipmentGenerator _equipmentGenerator;

        private Agent _tempAi;

        public CoopArenaController(
            IMessageBroker messageBroker, 
            INetworkAgentRegistry agentRegistry, 
            IRandomEquipmentGenerator equipmentGenerator)
        {
            _messageBroker = messageBroker;
            _agentRegistry = agentRegistry;
            _equipmentGenerator = equipmentGenerator;
            messageBroker.Subscribe<NetworkMissionJoinInfo>(Handle_JoinInfo);
        }

        ~CoopArenaController()
        {
            _messageBroker.Unsubscribe<NetworkMissionJoinInfo>(Handle_JoinInfo);
        }

        public override void AfterStart()
        {
            AddPlayerToArena();
        }

        private void Handle_JoinInfo(MessagePayload<NetworkMissionJoinInfo> payload)
        {
            Logger.Debug("Received join request");
            NetPeer netPeer = (NetPeer)payload.Who;

            NetworkMissionJoinInfo joinInfo = payload.What;

            Guid newAgentId = joinInfo.PlayerId;
            Vec3 startingPos = joinInfo.StartingPosition;

            Logger.Information("Spawning {EntityType} called {AgentName}({AgentID}) from {Peer}",
                joinInfo.CharacterObject.IsPlayerCharacter ? "Player" : "Agent",
                joinInfo.CharacterObject.Name, newAgentId, netPeer.EndPoint);

            Agent newAgent = SpawnAgent(startingPos, joinInfo.CharacterObject, true);
            _agentRegistry.RegisterNetworkControlledAgent(netPeer, joinInfo.PlayerId, newAgent);

            //Mission currentMission = Mission.Current;

            for (int i = 0; i < joinInfo.UnitIdString.Length; i++)
            {

                //PartyAgentOrigin partyAgentOrigin = new PartyAgentOrigin(PartyBase.MainParty, CharacterObject.Find(joinInfo.UnitIdString[i]));
                //Agent tempAi = currentMission.SpawnTroop(partyAgentOrigin, true, true, false, CharacterObject.Find(joinInfo.UnitIdString[i]).HasMount(), 1, 1, false, true, false, joinInfo.UnitStartingPosition[i], new Vec2());

                Agent tempAi = SpawnAgent(joinInfo.UnitStartingPosition[i], CharacterObject.Find(joinInfo.UnitIdString[i]), true);
                
                _agentRegistry.RegisterNetworkControlledAgent(netPeer, joinInfo.UnitId[i], tempAi);
            }
        }

        public void AddPlayerToArena()
        {
            // reset teams if any exists
            Mission.Current.ResetMission();

            Mission.Current.Teams.Add(BattleSideEnum.Defender, Hero.MainHero.MapFaction.Color, Hero.MainHero.MapFaction.Color2, null, true, false, true);
            Mission.Current.Teams.Add(BattleSideEnum.Attacker, Hero.MainHero.MapFaction.Color2, Hero.MainHero.MapFaction.Color, null, true, false, true);

            // players is attacker team
            Mission.Current.PlayerTeam = Mission.Current.AttackerTeam;  



            List<MatrixFrame> spawnFrames = (from e in Mission.Current.Scene.FindEntitiesWithTag("sp_arena")
                                             select e.GetGlobalFrame()).ToList();
            for (int i = 0; i < spawnFrames.Count; i++)
            {
                MatrixFrame value = spawnFrames[i];
                value.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
                spawnFrames[i] = value;
            }

            // get a random spawn point
            Random rand = new Random();
            MatrixFrame randomElement = spawnFrames[rand.Next(spawnFrames.Count)];  


            // spawn an instance of the player (controlled by default)
            SpawnPlayerAgent(CharacterObject.PlayerCharacter, randomElement);

            Agent.Main.SetTeam(Mission.Current.PlayerTeam, false);

            _tempAi = SpawnAgent(randomElement.origin, CharacterObject.Find("aserai_veteran_infantry"), false);

            for (int i = 1; i < Agent.Main.Team.TeamAgents.Count; i++)
            {
                CoopMissionNetworkBehavior._unitId.Add(Guid.NewGuid());
            }
        }

        
        // Spawn an agent based on its character object and frame. For now, Main agent character object is used
        // This should be the real character object in the future
        private static readonly PropertyInfo Hero_BattleEquipment = typeof(Hero).GetProperty("BattleEquipment", BindingFlags.Public | BindingFlags.Instance);
        private Agent SpawnPlayerAgent(CharacterObject character, MatrixFrame frame)
        {
            AgentBuildData agentBuildData = new AgentBuildData(character);
            agentBuildData.BodyProperties(character.GetBodyPropertiesMax());
            Mission mission = Mission.Current;
            agentBuildData = agentBuildData.Team(Mission.Current.PlayerTeam).InitialPosition(frame.origin);
            agentBuildData.NoHorses(true);

            Vec2 vec = frame.rotation.f.AsVec2;
            vec = vec.Normalized();
            Equipment generatedEquipment = _equipmentGenerator.CreateRandomEquipment(true);
            agentBuildData.Equipment(generatedEquipment);
            Hero_BattleEquipment.SetValue(character.HeroObject, generatedEquipment); 
            agentBuildData.InitialDirection(vec);
            agentBuildData.TroopOrigin(new SimpleAgentOrigin(character, -1, null, default));
            agentBuildData.Controller(Agent.ControllerType.Player);

            Agent agent = mission.SpawnAgent(agentBuildData);
            agent.FadeIn();

            return agent;
        }

        public Agent SpawnAgent(Vec3 startingPos, CharacterObject character, bool isEnemy)
        {
            AgentBuildData agentBuildData = new AgentBuildData(character);
            agentBuildData.BodyProperties(character.GetBodyPropertiesMax());
            agentBuildData.InitialPosition(startingPos);
            agentBuildData.Team(isEnemy ? Mission.Current.PlayerEnemyTeam : Mission.Current.PlayerTeam);
            agentBuildData.InitialDirection(Vec2.Forward);
            agentBuildData.NoHorses(true);
            agentBuildData.Equipment(character.IsHero ? character.HeroObject.BattleEquipment : character.Equipment);
            agentBuildData.TroopOrigin(new SimpleAgentOrigin(character, -1, null, default));
            agentBuildData.Controller(isEnemy ? Agent.ControllerType.None : Agent.ControllerType.AI);

            Agent agent = default;
            GameLoopRunner.RunOnMainThread(() =>
            {
                agent = Mission.Current.SpawnAgent(agentBuildData);
                agent.FadeIn();
            }, true);

            if (agent.IsAIControlled)
            {
                agent.SetWatchState(Agent.WatchState.Alarmed);
            }

            return agent;
        }

        // DEBUG METHOD: Starts an Arena fight
        public void StartArenaFight()
        {
            // reset teams if any exists
            Mission.Current.ResetMission();

            Mission.Current.Teams.Add(BattleSideEnum.Defender, Hero.MainHero.MapFaction.Color, Hero.MainHero.MapFaction.Color2, null, true, false, true);
            Mission.Current.Teams.Add(BattleSideEnum.Attacker, Hero.MainHero.MapFaction.Color2, Hero.MainHero.MapFaction.Color, null, true, false, true);

            // players is defender team
            Mission.Current.PlayerTeam = Mission.Current.DefenderTeam;


            // find areas of spawn
            List<MatrixFrame> spawnFrames = (from e in Mission.Current.Scene.FindEntitiesWithTag("sp_arena")
                                             select e.GetGlobalFrame()).ToList();
            for (int i = 0; i < spawnFrames.Count; i++)
            {
                MatrixFrame value = spawnFrames[i];
                value.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
                spawnFrames[i] = value;
            }
            // get a random spawn point
            MatrixFrame randomElement = spawnFrames.GetRandomElement();


            // spawn an instance of the player (controlled by default)
            SpawnPlayerAgent(CharacterObject.PlayerCharacter, randomElement);
        }
    }
}