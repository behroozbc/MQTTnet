namespace MQTTnet.Protocol
{
    public enum MqttAuthenticateReasonCode
    {
        Success = 0x00,
        ContinueAuthentication = 0x18,
        ReAuthenticate = 0x19
    }
}
