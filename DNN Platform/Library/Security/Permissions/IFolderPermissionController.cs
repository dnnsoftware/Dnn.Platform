using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Security.Permissions
{
    public interface IFolderPermissionController
    {
        bool CanAddFolder(IFolderInfo folder);
        bool CanAdminFolder(IFolderInfo folder);
        bool CanViewFolder(IFolderInfo folder);
    }
}
