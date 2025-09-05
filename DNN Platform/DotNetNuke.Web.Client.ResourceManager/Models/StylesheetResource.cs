using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    public class StylesheetResource : FileInclude, IStylesheetResource
    {
        private readonly IClientResourcesController _clientResourcesController;
        public StylesheetResource(IClientResourcesController clientResourcesController)
        {
            this._clientResourcesController = clientResourcesController;
        }

        public bool Disabled { get; set; } = false;
        public string Src { get; set; }
        public string Key { get; set; }

        public void Register()
        {
            this._clientResourcesController.AddStylesheet(this);
        }

        public string Render()
        {
            var htmlString = "<link";
            htmlString += $" href=\"{this.Src}\"";
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
