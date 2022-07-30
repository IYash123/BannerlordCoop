﻿using Common;
using Common.Messaging;
using Common.Serialization;
using Coop.NetImpl.LiteNet;
using LiteNetLib;
using LiteNetLib.Utils;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Coop.Mod.Mission.Network
{
    public class MessageBroker : IMessageBroker, IPacketHandler
    {
        private readonly Dictionary<Type, List<Delegate>> m_Subscribers;
        private readonly LiteNetP2PClient m_Client;

        public PacketType PacketType => PacketType.Event;

        public MessageBroker(LiteNetP2PClient client)
        {
            m_Client = client;
            m_Subscribers = new Dictionary<Type, List<Delegate>>();

            m_Client.AddHandler(this);
        }

        public void Publish<T>(object source, T message)
        {
            if (message == null || source == null)
                return;

            var payload = new MessagePayload<T>(message, source.ToString());

            EventPacket messagePacket = new EventPacket(payload);

            m_Client.SendAll(messagePacket);
        }

        public void Subscribe<T>(Action<MessagePayload<T>> subscription)
        {
            var delegates = m_Subscribers.ContainsKey(typeof(T)) ?
                            m_Subscribers[typeof(T)] : new List<Delegate>();
            if (!delegates.Contains(subscription))
            {
                delegates.Add(subscription);
            }
            m_Subscribers[typeof(T)] = delegates;
        }

        public void Unsubscribe<T>(Action<MessagePayload<T>> subscription)
        {
            if (!m_Subscribers.ContainsKey(typeof(T))) return;
            var delegates = m_Subscribers[typeof(T)];
            if (delegates.Contains(subscription))
                delegates.Remove(subscription);
            if (delegates.Count == 0)
                m_Subscribers.Remove(typeof(T));
        }

        public void Dispose()
        {
            m_Subscribers?.Clear();
        }

        public void HandlePacket(NetPeer peer, IPacket packet)
        {
            object payload = CommonSerializer.Deserialize(packet.Data, SerializationMethod.ProtoBuf);

            Type type = payload.GetType();
            if (type.GetGenericTypeDefinition() != typeof(MessagePayload<>))
            {
                throw new InvalidCastException($"{payload.GetType()} is not of type {typeof(MessagePayload<>)}");
            }

            Type T = type.GetProperty("What").PropertyType;

            if (!m_Subscribers.ContainsKey(T))
            {
                return;
            }

            var delegates = m_Subscribers[T];
            if (delegates == null || delegates.Count == 0) return;

            foreach (var handler in delegates)
            {
                Task.Factory.StartNew(() => handler.Method.Invoke(handler.Target, new object[] { payload }));
            }
        }
    }
}