using System.Collections.Generic;
using MQTTnet.Packets;

namespace MQTTnet.Server
{
    public sealed class MqttReceiveAuthenticationDataResult
    {
        public byte[] AuthenticationData { get; set; }
        
        public List<MqttUserProperty> UserProperties { get; set; }
    }
}