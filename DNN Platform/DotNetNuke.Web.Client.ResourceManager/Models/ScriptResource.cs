using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    public class ScriptResource : FileInclude, IScriptResource
    {
        private readonly IClientResourcesController _clientResourcesController;

        public bool Async { get; set; } = false;

        public bool Defer { get; set; } = false;

        public bool NoModule { get; set; } = false;

        public ScriptResource(IClientResourcesController clientResourcesController)
        {
            this._clientResourcesController = clientResourcesController;
        }

        public void Register()
        {
            this._clientResourcesController.AddScript(this);
        }
    }
}
