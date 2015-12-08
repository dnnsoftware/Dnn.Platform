using System.Configuration;

namespace DotNetNuke.Web.ConfigSection
{
    public class AuthServicesConfiguration : ConfigurationSection
    {
        public static AuthServicesConfiguration GetConfig()
        {
            return ConfigurationManager.GetSection("authServices") as AuthServicesConfiguration ??
                new AuthServicesConfiguration();
        }

        [ConfigurationProperty("messageHandlers", IsRequired = true)]
        public MessageHandlersCollection MessageHandlers =>
            this["messageHandlers"] as MessageHandlersCollection ?? new MessageHandlersCollection();
    }
}
