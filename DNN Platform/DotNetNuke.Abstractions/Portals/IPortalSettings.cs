namespace DotNetNuke.Abstractions.Portals
{
    public interface IPortalSettings
    {
        int PortalId { get; }
        bool ContentLocalizationEnabled { get; }
        bool EnableUrlLanguage { get; }
        bool SSLEnabled { get; }
        string SSLURL { get; }
    }
}
