// ReSharper disable once CheckNamespace
using System;
using System.Web.UI;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Tokens
{
    public class StylesheetDto
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }
    }

    public class CssPropertyAccess : JsonPropertyAccess<StylesheetDto>
    {
        private readonly Page _page;

        public CssPropertyAccess(Page page)
        {
            _page = page;
        }

        protected override string ProcessToken(StylesheetDto model, UserInfo accessingUser, Scope accessLevel)
        {
            if (String.IsNullOrEmpty(model.Path))
            {
                throw new ArgumentException("The Css token must specify a path or property.");
            }
            if (model.Priority == 0)
            {
                model.Priority = (int)FileOrder.Css.DefaultPriority;
            }
            if (String.IsNullOrEmpty(model.Provider))
            {
                ClientResourceManager.RegisterStyleSheet(_page, model.Path, model.Priority);
            }
            else
            {
                ClientResourceManager.RegisterStyleSheet(_page, model.Path, model.Priority, model.Provider);
            }

            return String.Empty;
        }
    }
}
