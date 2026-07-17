using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.MvcPipeline.ModuleControl;

namespace Dnn.ContactList.Mvc.Controls
{
    public class ViewControl : MvcModuleControl, IActionable
    {
        // IActionable implementation to add module actions
        public ModuleActionCollection ModuleActions
        {
            get
            {
                return new ModuleActionCollection
                {
                    {
                        this.GetNextActionID(),
                        Localization.GetString("AddContact", this.LocalResourceFile),
                        ModuleActionType.AddContent,
                        string.Empty,
                        string.Empty,
                        this.EditUrl(),
                        false,
                        SecurityAccessLevel.Edit,
                        true,
                        false
                    }
                };
            }
        }
    }
}
