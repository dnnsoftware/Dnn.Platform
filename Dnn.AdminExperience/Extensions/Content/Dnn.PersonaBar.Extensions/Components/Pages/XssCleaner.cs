using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Security;

namespace Dnn.PersonaBar.Pages.Components
{
    public static class XssCleaner
    {
        public static void Clean(this PageSettings input)
        {
            input.Title = Clean(input.Title);
            input.Description = Clean(input.Description);
            input.Name = Clean(input.Name);
            input.Keywords = Clean(input.Keywords);
            input.Tags = Clean(input.Tags);
            input.Url = Clean(input.Url);
            input.PageType = Clean(input.PageType);
            input.Alias = Clean(input.Alias);
            input.LocalizedName = Clean(input.LocalizedName);
            input.PageStyleSheet = Clean(input.PageStyleSheet);
        }

        public static void Clean(this BulkPage input)
        {
            input.BulkPages = Clean(input.BulkPages);
            input.Keywords = Clean(input.Keywords);
            input.Tags = Clean(input.Tags);
        }

        public static void Clean(this PageTemplate input)
        {
            input.Description = Clean(input.Description);
            input.Name = Clean(input.Name);
        }

        public static string Clean(string input,
            PortalSecurity.FilterFlag filterFlag = PortalSecurity.FilterFlag.NoMarkup)
        {
            return PortalSecurity.Instance.InputFilter(input, filterFlag);
        }
    }
}