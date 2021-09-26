using MQTTnet.Packets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MQTTnet.PacketDispatcher
{
    public sealed class MqttPacketDispatcher
    {
        readonly List<IMqttPacketAwaitable> _awaitables = new List<IMqttPacketAwaitable>(1024);

        readonly object _authPacketHandlerSyncRoot = new object();
        Func<MqttAuthPacket, Task> _authPacketHandler;

        public void SetAuthPacketListener(Func<MqttAuthPacket, Task> authPacketHandler)
        {
            if (authPacketHandler == null) throw new ArgumentNullException(nameof(authPacketHandler));
            
            lock (_authPacketHandlerSyncRoot)
            {
                if (_authPacketHandler != null)
                {
                    throw new InvalidOperationException("An AUTH packet listener is already active.");
                }

                _authPacketHandler = authPacketHandler;
            }
        }
        
        public void FailAll(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            lock (_awaitables)
            {
                foreach (var awaitable in _awaitables)
                {
                    awaitable.Fail(exception);
                }

                _awaitables.Clear();
            }
        }

        public void CancelAll()
        {
            lock (_awaitables)
            {
                foreach (var awaitable in _awaitables)
                {
                    awaitable.Cancel();
                }

                _awaitables.Clear();
            }
        }
        
        public bool TryDispatch(MqttBasePacket packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet));

            if (packet is MqttAuthPacket authPacket)
            {
                if (_authPacketHandler != null)
                {
                    _authPacketHandler.Invoke(authPacket);
                    return true;
                }
            }
            
            ushort identifier = 0;
            if (packet is IMqttPacketWithIdentifier packetWithIdentifier)
            {
                identifier = packetWithIdentifier.PacketIdentifier;
            }

            var packetType = packet.GetType();
            var awaitables = new List<IMqttPacketAwaitable>();
            
            lock (_awaitables)
            {
                for (var i = _awaitables.Count - 1; i >= 0; i--)
                {
                    var entry = _awaitables[i];

                    // Note: The PingRespPacket will also arrive here and has NO identifier but there
                    // is code which waits for it. So the code must be able to deal with filters which
                    // are referring to the type only (identifier is 0)!
                    if (entry.Filter.Type != packetType || entry.Filter.Identifier != identifier)
                    {
                        continue;
                    }
                    
                    awaitables.Add(entry);
                    _awaitables.RemoveAt(i);
                }
            }
            
            foreach (var matchingEntry in awaitables)
            {
                matchingEntry.Complete(packet);
            }

            return awaitables.Count > 0;
        }
        
        public MqttPacketAwaitable<TResponsePacket> AddAwaitable<TResponsePacket>(ushort packetIdentifier) where TResponsePacket : MqttBasePacket
        {
            var awaitable = new MqttPacketAwaitable<TResponsePacket>(packetIdentifier, this);

            lock (_awaitables)
            {
                _awaitables.Add(awaitable);
            }
            
            return awaitable;
        }

        public void RemoveAwaitable(IMqttPacketAwaitable awaitable)
        {
            if (awaitable == null) throw new ArgumentNullException(nameof(awaitable));
            
            lock (_awaitables)
            {
                _awaitables.Remove(awaitable);
            }
        }

        public void RemoveAuthPacketListener()
        {
            _authPacketHandler = null;
        }
    }
}