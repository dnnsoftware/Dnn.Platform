namespace DotNetNuke.Web.Api.Internal
{
    internal class AntiForgeryImpl : IAntiForgery
    {
        public string CookieName { get { return System.Web.Helpers.AntiForgeryConfig.CookieName; } }
        public void Validate(string cookieToken, string headerToken)
        {
            System.Web.Helpers.AntiForgery.Validate(cookieToken, headerToken);
        }
    }
}