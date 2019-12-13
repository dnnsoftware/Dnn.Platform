namespace DotNetNuke.Security.Permissions
{
    public class CorePermissionProvider : PermissionProvider
    {
        public override bool SupportsFullControl()
        {
            return false;
        }
    }
}
