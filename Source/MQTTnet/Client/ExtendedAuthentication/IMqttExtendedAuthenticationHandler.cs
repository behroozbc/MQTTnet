using System.Threading.Tasks;

namespace MQTTnet.Client.ExtendedAuthentication
{
    public interface IMqttExtendedAuthenticationHandler
    {
        Task StartExtendedAuthentication();

        Task EndExtendedAuthentication();

        Task StartReAuthentication(MqttExtendedAuthenticationRequest initialRequest);
        
        Task EndReAuthentication();
        
        Task HandleExtendedAuthenticationAsync(MqttExtendedAuthenticationContext context);
    }
}
