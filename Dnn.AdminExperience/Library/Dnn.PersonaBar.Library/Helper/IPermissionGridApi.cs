using System.Net.Http;

namespace Dnn.PersonaBar.Library.Helper
{
    public interface IPermissionGridApi
    {
        // TODO: ? HttpResponseMessage GetPermissionsData();

        /// <summary>
        /// Returns all roles/role groups info
        /// </summary>
        /// <returns></returns>
        HttpResponseMessage GetRoles();
    }
}
