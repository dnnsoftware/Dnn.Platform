using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    public class LinkResource : FileInclude, ILinkResource
    {
        private readonly IClientResourcesController _clientResourcesController;
        public LinkResource(IClientResourcesController clientResourcesController)
        {
            this._clientResourcesController = clientResourcesController;
        }

        public bool Disabled { get; set; }

        public void Register()
        {
            this._clientResourcesController.AddLink(this);
        }
    }
}
