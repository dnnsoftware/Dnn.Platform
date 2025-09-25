using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    public class StylesheetResource : ResourceBase, IStylesheetResource
    {
        private readonly IClientResourcesController _clientResourcesController;
        public StylesheetResource(IClientResourcesController clientResourcesController)
        {
            this._clientResourcesController = clientResourcesController;
        }

        public bool Disabled { get; set; } = false;

        public void Register()
        {
            this._clientResourcesController.AddStylesheet(this);
        }

        public string Render(int crmVersion, bool useCdn, string applicationPath)
        {
            var htmlString = "<link";
            htmlString += $" href=\"{this.GetVersionedPath(crmVersion, useCdn, applicationPath)}\"";
            if (this.Preload)
            {
                htmlString += $" rel=\"preload\" as=\"style\"";
            }
            else
            {
                htmlString += $" rel=\"stylesheet\"";
            }
            if (this.Disabled)
            {
                htmlString += " disabled";
            }
            htmlString += this.RenderBlocking();
            htmlString += this.RenderCrossOriginAttribute();
            htmlString += this.RenderFetchPriority();
            htmlString += this.RenderIntegrity();
            htmlString += this.RenderReferrerPolicy();
            htmlString += this.RenderAttributes();
            htmlString += " />";
            return htmlString;
        }
    }
}
