using System;
using System.Collections.Generic;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace MQTTnet.Client.ExtendedAuthentication
{
    public sealed class MqttExtendedAuthenticationRequest
    {
        readonly MqttAuthPacket _authPacket;

        public MqttExtendedAuthenticationRequest(MqttAuthPacket authPacket)
        {
            _authPacket = authPacket ?? throw new ArgumentNullException(nameof(authPacket));
        }

        public List<MqttUserProperty> UserProperties => _authPacket.Properties.UserProperties;

        public byte[] AuthenticationData => _authPacket.Properties.AuthenticationData;

        /// <summary>
        /// The reason for the disconnect.
        /// This Reason String is human readable, designed for diagnostics and SHOULD NOT be parsed by the receiver.
        /// </summary>
        public string ReasonString => _authPacket.Properties.ReasonString;

        public bool ContinueAuthentication => _authPacket.ReasonCode == MqttAuthenticateReasonCode.ContinueAuthentication;
    }
}