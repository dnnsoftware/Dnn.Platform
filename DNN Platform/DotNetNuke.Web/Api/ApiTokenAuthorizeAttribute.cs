namespace DotNetNuke.Web.Api
{
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.Api.Auth.ApiTokens;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    public class ApiTokenAuthorizeAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        public string Key { get; set; } = "";
        public string ResourceFile { get; set; } = "";
        public ApiTokenScope Scope { get; set; }

        public ApiTokenAuthorizeAttribute(string key, string resourceFile, ApiTokenScope scope)
        {
            Key = key;
            ResourceFile = resourceFile;
            Scope = scope;
        }

        public string Purpose
        {
            get
            {
                return DotNetNuke.Services.Localization.Localization.GetString(Key + ".Text", ResourceFile);
            }
        }

        public override bool IsAuthorized(AuthFilterContext context)
        {
            var token = ApiTokenController.Instance.GetCurrentThreadApiToken();
            if (token == null)
            {
                return false;
            }

            var scopeMatch = false;
            switch (this.Scope)
            {
                case ApiTokenScope.Host:
                    scopeMatch = token.Scope == ApiTokenScope.Host;
                    break;
                case ApiTokenScope.Portal:
                    scopeMatch = token.Scope == ApiTokenScope.Portal || token.Scope == ApiTokenScope.Host;
                    break;
                case ApiTokenScope.User:
                    scopeMatch = token.Scope == ApiTokenScope.User && UserController.Instance.GetCurrentUserInfo() != null;
                    break;
            }

            if (scopeMatch)
            {
                return token.TokenKeys.Contains(this.Key.ToLowerInvariant());
            }

            return false;
        }
    }
}
