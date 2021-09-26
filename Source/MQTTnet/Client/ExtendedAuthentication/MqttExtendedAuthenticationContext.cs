using System;
using MQTTnet.Packets;

namespace MQTTnet.Client.ExtendedAuthentication
{
    public sealed class MqttExtendedAuthenticationContext
    {
        readonly MqttAuthPacket _authPacket;

        public MqttExtendedAuthenticationContext(MqttAuthPacket authPacket)
        {
            _authPacket = authPacket ?? throw new ArgumentNullException(nameof(authPacket));
            
            Request = new MqttExtendedAuthenticationRequest(authPacket);
        }

        public string AuthenticationMethod => _authPacket.Properties.AuthenticationMethod;

        public MqttExtendedAuthenticationRequest Request { get; }

        public MqttExtendedAuthenticationData Response { get; set; }

    }
}