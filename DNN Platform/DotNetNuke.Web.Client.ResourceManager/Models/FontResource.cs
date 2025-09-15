using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    public class FontResource : ResourceBase, IFontResource
    {
        private readonly IClientResourcesController _clientResourcesController;
        public FontResource(IClientResourcesController clientResourcesController)
        {
            this._clientResourcesController = clientResourcesController;
        }

        public string ResolvedPath { get; set; }

        public void Register()
        {
            this._clientResourcesController.AddFont(this);
        }

        public string Render(int crmVersion, bool useCdn)
        {
            var htmlString = "<link";
            htmlString += $" href=\"{this.GetVersionedPath(crmVersion, useCdn)}\"";
            if (this.Preload)
            {
                htmlString += $" rel=\"preload\" as=\"font\"";
            }
            else
            {
                htmlString += $" rel=\"font\"";
            }
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
