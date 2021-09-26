using System.Threading.Tasks;

namespace MQTTnet.Client.ExtendedAuthentication
{
    public interface IMqttExtendedAuthenticationHandler
    {
        Task StartExtendedAuthentication(MqttExtendedAuthenticationRequest initialRequest);

        Task EndExtendedAuthentication();
        
        Task HandleExtendedAuthenticationAsync(MqttExtendedAuthenticationContext context);
    }
}
