using System.Web.UI;

namespace DotNetNuke.Services.Tokens
{
    public class HtmlTokenReplace : TokenReplace
    {
        public HtmlTokenReplace(Page page)
            : base(Scope.DefaultSettings)
        {
            PropertySource["css"] = new CssPropertyAccess(page);
            PropertySource["js"] = new JavaScriptPropertyAccess(page);
            PropertySource["javascript"] = new JavaScriptPropertyAccess(page);
            PropertySource["antiforgerytoken"] = new AntiForgeryTokenPropertyAccess();
        }
    }
}
