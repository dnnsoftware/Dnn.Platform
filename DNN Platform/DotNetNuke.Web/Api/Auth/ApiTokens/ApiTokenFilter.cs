namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    public enum ApiTokenFilter
    {
        All = 0,
        Active = 1,
        Revoked = 2,
        Expired = 3,
        RevokedOrExpired = 4
    }
}
