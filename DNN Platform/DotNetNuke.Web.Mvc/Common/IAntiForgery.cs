namespace DotNetNuke.Web.Mvc.Common
{
    public interface IAntiForgery
    {
        string CookieName { get; }
        void Validate(string cookieToken, string headerToken);
    }
}