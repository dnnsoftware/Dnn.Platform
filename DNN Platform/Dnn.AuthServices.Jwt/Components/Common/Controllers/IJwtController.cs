using System.Net.Http;
using Dnn.AuthServices.Jwt.Components.Entity;

namespace Dnn.AuthServices.Jwt.Components.Common.Controllers
{
    public interface IJwtController
    {
        string SchemeType { get; }
        string ValidateToken(HttpRequestMessage request);
        bool LogoutUser(HttpRequestMessage request);
        LoginResultData LoginUser(HttpRequestMessage request, LoginData loginData);
        LoginResultData RenewToken(HttpRequestMessage request, string renewalToken);
    }
}
