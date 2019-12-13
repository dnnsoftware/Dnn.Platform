using System.Web;

namespace DotNetNuke.Services.UserRequest
{
    public interface IUserRequestIPAddressController
    {
        /// <summary>
        ///  To retrieve IPv4 of user making request to application.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>IP address</returns>
        string GetUserRequestIPAddress(HttpRequestBase request);

        /// <summary>
        ///  To retrieve IPv4/IPv6 of user making request to application
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ipFamily"></param>
        /// <returns>IP address</returns>
        string GetUserRequestIPAddress(HttpRequestBase request, IPAddressFamily ipFamily);
    }
}
