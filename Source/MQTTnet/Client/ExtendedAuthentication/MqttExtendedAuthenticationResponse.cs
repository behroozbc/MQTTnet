using System.Collections.Generic;
using MQTTnet.Packets;

namespace MQTTnet.Client.ExtendedAuthentication
{
    public sealed class MqttExtendedAuthenticationResponse
    {
        public byte[] AuthenticationData { get; set; }

        public List<MqttUserProperty> UserProperties { get; } = new List<MqttUserProperty>();
    }
}