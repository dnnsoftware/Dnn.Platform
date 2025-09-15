using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    public class ScriptResource : ResourceBase, IScriptResource
    {
        private readonly IClientResourcesController _clientResourcesController;

        public bool Async { get; set; } = false;

        public bool Defer { get; set; } = false;

        public bool NoModule { get; set; } = false;
        public string Type { get; set; } = "";

        public ScriptResource(IClientResourcesController clientResourcesController)
        {
            this._clientResourcesController = clientResourcesController;
        }

        public void Register()
        {
            this._clientResourcesController.AddScript(this);
        }

        public string Render(int crmVersion, bool useCdn)
        {
            var htmlString = "<script";
            htmlString += $" src=\"{this.GetVersionedPath(crmVersion, useCdn)}\"";
            if (this.Async)
            {
                htmlString += " async";
            }
            if (this.Defer)
            {
                htmlString += " defer";
            }
            if (this.NoModule)
            {
                htmlString += " nomodule";
            }
            if (!string.IsNullOrEmpty(this.Type))
            {
                htmlString += $" type=\"{this.Type}\"";
            }

            htmlString += this.RenderBlocking();
            htmlString += this.RenderCrossOriginAttribute();
            htmlString += this.RenderFetchPriority();
            htmlString += this.RenderIntegrity();
            htmlString += this.RenderReferrerPolicy();
            htmlString += this.RenderAttributes();
            htmlString += " type=\"text/javascript\"></script>";
            return htmlString;
        }
    }
}
