using DotNetNuke.Entities.Users;

namespace DotNetNuke.Services.ImprovementsProgram
{
    public interface IBeaconService
    {
        string GetBeaconEndpoint();
        string GetBeaconQuery(UserInfo user, string filePath = null);
        string GetBeaconUrl(UserInfo user, string filePath = null);
        bool IsBeaconEnabledForControlBar(UserInfo user);
        bool IsBeaconEnabledForPersonaBar();
    }
}