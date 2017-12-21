namespace Dnn.PersonaBar.Library.Security
{
    /// <summary>
    /// Global level security service to do permission check.
    /// </summary>
    public interface ISecurityService
    {
        bool IsPagesAdminUser();
    }
}
