﻿using Common;
using Common.Logging;
using Common.Messaging;
using LiteNetLib;
using Missions.Services.Agents.Messages;
using Missions.Services.Network;
using Missions.Services.Network.Messages;
using ProtoBuf;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Missions.Services.Agents.Packets
{
    [ProtoContract]
    public readonly struct MovementPacket : IPacket
    {
        public DeliveryMethod DeliveryMethod => DeliveryMethod.Unreliable;

        public PacketType PacketType => PacketType.Movement;

        public byte[] Data => new byte[0];

        [ProtoMember(1)]
        public AgentData Agent { get; }
        [ProtoMember(2)]
        public Guid AgentId { get; }

        public MovementPacket(Guid agentGuid, Agent agent)
        {
            AgentId = agentGuid;
            Agent = new AgentData(agent);
        }

        public MovementPacket(Guid agentGuid, AgentData agentData)
        {
            AgentId = agentGuid;
            Agent = agentData;
        }

        public void Apply(Agent agent)
        {
            Agent.Apply(agent);
        }
    }

    public class MovementHandler : IPacketHandler, IDisposable
    {
        private const int PACKETS = 30;
        private readonly static TimeSpan TIME_BETWEEN_PACKETS = TimeSpan.FromSeconds(1);

        private static int PACKET_UPDATE_RATE = (int)Math.Round(TIME_BETWEEN_PACKETS.TotalMilliseconds / PACKETS);

        private static readonly ILogger Logger = LogManager.GetLogger<LiteNetP2PClient>();

        private readonly LiteNetP2PClient _client;
        private readonly IMessageBroker _messageBroker;
        private readonly INetworkAgentRegistry _agentRegistry;

        private Dictionary<Guid, AgentMovementDelta> _agentMovementDeltas = new Dictionary<Guid, AgentMovementDelta>();

        private Timer _senderTimer;

        public MovementHandler(LiteNetP2PClient client, IMessageBroker messageBroker, INetworkAgentRegistry agentRegistry)
        {

            _client = client;
            _messageBroker = messageBroker;
            _agentRegistry = agentRegistry;

            _messageBroker.Subscribe<PeerDisconnected>(Handle_PeerDisconnect);
            _messageBroker.Subscribe<ActionDataChanged>(Handle_ActionDataChanged);
            _messageBroker.Subscribe<LookDirectionChanged>(Handle_LookDirectionChanged);
            _messageBroker.Subscribe<MountDataChanged>(Handle_MountDataChanged);
            _messageBroker.Subscribe<MovementInputVectorChanged>(Handle_MovementInputVectorChanged);

            _client.AddHandler(this);

            // start the SendMessage every PACKET_UPDATE_RATE milliseconds, TIME_BETWEEN_PACKETS.Seconds after it was initialized
            _senderTimer = new Timer(SendMessage, null, TIME_BETWEEN_PACKETS.Seconds, PACKET_UPDATE_RATE);
        }

        ~MovementHandler()
        {
            Dispose();
        }

        public void Dispose()
        {
            _client.RemoveHandler(this);
            _messageBroker.Unsubscribe<PeerDisconnected>(Handle_PeerDisconnect);
            _messageBroker.Unsubscribe<ActionDataChanged>(Handle_ActionDataChanged);
            _messageBroker.Unsubscribe<LookDirectionChanged>(Handle_LookDirectionChanged);
            _messageBroker.Unsubscribe<MountDataChanged>(Handle_MountDataChanged);
            _messageBroker.Unsubscribe<MovementInputVectorChanged>(Handle_MovementInputVectorChanged);
            _senderTimer?.Dispose();
        }

        public PacketType PacketType => PacketType.Movement;

        Mission CurrentMission
        {
            get
            {
                Mission current = null;
                GameLoopRunner.RunOnMainThread(() =>
                {
                    current = Mission.Current;
                });
                return current;
            }
        }

        private void Handle_ActionDataChanged(MessagePayload<ActionDataChanged> payload)
        {
            var delta = GetDelta(payload.What);

            delta.CalculateMovement(payload.What);
        }

        private void Handle_LookDirectionChanged(MessagePayload<LookDirectionChanged> payload)
        {
            var delta = GetDelta(payload.What);

            delta.CalculateMovement(payload.What);
        }    

        private void Handle_MountDataChanged(MessagePayload<MountDataChanged> payload)
        {
            var delta = GetDelta(payload.What);

            delta.CalculateMovement(payload.What);
        }

        private void Handle_MovementInputVectorChanged(MessagePayload<MovementInputVectorChanged> payload)
        {
            var delta = GetDelta(payload.What);

            delta.CalculateMovement(payload.What);
        }

        private AgentMovementDelta GetDelta(IMovement payload)
        {
            if (_agentMovementDeltas.TryGetValue(payload.Guid, out var delta))
            {
                return delta;
            }

            delta = new AgentMovementDelta(payload.Agent, payload.Guid);

            _agentMovementDeltas.Add(payload.Guid, delta);

            return delta;
        }

        private IEnumerable<AgentMovementDelta> PopAllDeltas()
        {
            foreach (var kv in _agentMovementDeltas)
            {
                yield return kv.Value;
            }

            _agentMovementDeltas.Clear();
        }

        private void SendMessage(object state)
        {
            foreach (var delta in PopAllDeltas())
            {
                _client.SendAll(delta.GetPacket());
            }
        }

        public void HandlePacket(NetPeer peer, IPacket packet)
        {
            if (_agentRegistry.OtherAgents.TryGetValue(peer, out AgentGroupController agentGroupController))
            {
                MovementPacket movement = (MovementPacket)packet;
                agentGroupController.ApplyMovement(movement);
            }
        }

        public void Handle_PeerDisconnect(MessagePayload<PeerDisconnected> payload)
        {
            if (Mission.Current == null) return;

            NetPeer peer = payload.What.NetPeer;

            Logger.Debug("Handling disconnect for {peer}", peer);

            if (_agentRegistry.OtherAgents.TryGetValue(peer, out AgentGroupController controller))
            {
                foreach (var kv in controller.ControlledAgents)
                {
                    var agent = kv.Value;
                    var guid = kv.Key;

                    GameLoopRunner.RunOnMainThread(() =>
                    {
                        agent.MakeDead(false, ActionIndexCache.act_none);
                        agent.FadeOut(false, true);
                    });

                    _agentMovementDeltas.Remove(guid);
                }

                _agentRegistry.RemovePeer(peer);
            }
        }

        public static Vec2 InterpolatePosition(Vec2 controlInput, Vec3 rotation, Vec2 currentPosition, Vec2 newPosition)
        {
            Vec2 directionVector = newPosition - currentPosition;
            double angle = Math.Atan2(rotation.y, rotation.x);
            directionVector = Rotate(directionVector, angle);

            return directionVector;
        }

        public static Vec2 Rotate(Vec2 v, double radians)
        {
            float sin = MathF.Sin((float)radians);
            float cos = MathF.Cos((float)radians);

            float tx = v.x;
            float ty = v.y;
            v.x = cos * tx - sin * ty;
            v.y = sin * tx + cos * ty;
            return v;
        }
    }
}
