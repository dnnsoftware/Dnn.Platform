namespace DotNetNuke.Web.Api.Internal
{
    public interface IAntiForgery
    {
        string CookieName { get; }
        void Validate(string cookieToken, string headerToken);
    }
}