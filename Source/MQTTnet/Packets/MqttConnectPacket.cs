namespace MQTTnet.Packets
{
    public sealed class MqttConnectPacket : MqttBasePacket
    {
        public string ClientId { get; set; }

        public string Username { get; set; }

        public byte[] Password { get; set; }

        public ushort KeepAlivePeriod { get; set; }

        /// <summary>
        /// Also called "Clean Start" in MQTTv5.
        /// </summary>
        public bool CleanSession { get; set; }

        public MqttApplicationMessage WillMessage { get; set; }

        /// <summary>
        /// Added in MQTT 5.0.0.
        /// </summary>
        public MqttConnectPacketProperties Properties { get; set; } = new MqttConnectPacketProperties();

        public override string ToString()
        {
            return string.Concat("Connect: [ClientId=", ClientId, "] [Username=", Username, "] [Password length=", Password?.Length ?? 0, "] [KeepAlivePeriod=", KeepAlivePeriod, "] [CleanSession=", CleanSession, "]");
        }
    }
}
