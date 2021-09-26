using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Adapter;
using MQTTnet.Exceptions;
using MQTTnet.Formatter;
using MQTTnet.Implementations;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace MQTTnet.Server
{
    public sealed class MqttExtendedAuthenticationHandler
    {
        readonly MqttConnectionValidatorContext _connectionValidatorContext;
        readonly IMqttChannelAdapter _channelAdapter;

        public MqttExtendedAuthenticationHandler(MqttConnectionValidatorContext connectionValidatorContext, IMqttChannelAdapter channelAdapter)
        {
            _connectionValidatorContext = connectionValidatorContext ?? throw new ArgumentNullException(nameof(connectionValidatorContext));
            _channelAdapter = channelAdapter ?? throw new ArgumentNullException(nameof(channelAdapter));
        }

        public bool IsSupported => _connectionValidatorContext.ProtocolVersion >= MqttProtocolVersion.V500;

        public Task SendAuthenticationDataAsync(byte[] authenticationData, bool continueAuthentication, CancellationToken cancellationToken)
        {
            if (authenticationData == null) throw new ArgumentNullException(nameof(authenticationData));
            
            ThrowIfNotSupported();

            var authPacket = new MqttAuthPacket
            {
                ReasonCode = continueAuthentication ? MqttAuthenticateReasonCode.ContinueAuthentication : MqttAuthenticateReasonCode.Success,
                Properties = new MqttAuthPacketProperties
                {
                    AuthenticationMethod = _connectionValidatorContext.AuthenticationMethod,
                    AuthenticationData = authenticationData
                }
            };

            return _channelAdapter.SendPacketAsync(authPacket, cancellationToken);
        }
        
        public async Task<MqttReceiveAuthenticationDataResult> ReceiveAuthenticationDataAsync(CancellationToken cancellationToken)
        {
            ThrowIfNotSupported();
            
            var receivedPacket = await _channelAdapter.ReceivePacketAsync(cancellationToken);

            if (!(receivedPacket is MqttAuthPacket authPacket))
            {
                throw new MqttProtocolViolationException($"Expected AUTH packet but got {receivedPacket.GetType().Name} packet.");
            }

            if (!string.Equals(authPacket.Properties?.AuthenticationMethod, _connectionValidatorContext.AuthenticationMethod))
            {
                throw new MqttProtocolViolationException("The authentication method cannot be changed while authenticating.");
            }

            return new MqttReceiveAuthenticationDataResult
            {
                AuthenticationData = authPacket.Properties?.AuthenticationData ?? PlatformAbstractionLayer.EmptyByteArray,
                UserProperties = authPacket.Properties?.UserProperties
            };
        }

        void ThrowIfNotSupported()
        {
            if (!IsSupported)
            {
                throw new NotSupportedException("Extended authenticated requires MQTT version 5.0.0 or greater.");
            }

            if (string.IsNullOrEmpty(_connectionValidatorContext.AuthenticationMethod))
            {
                throw new MqttProtocolViolationException("Extended authentication requires a valid authentication method.");
            }
        }
    }
}