using System;
using System.Web.UI;
using ClientDependency.Core;
using ClientDependency.Core.Controls;

namespace DotNetNuke.Web.Client.Controls
{
    public abstract class ClientResourceExclude : Control
    {

        public string Name { get; set; }

        public ClientDependencyType DependencyType { get; internal set; }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            var loader = Page.FindControl("ClientResourceIncludes");
            Name = Name.ToLower();

            if (loader != null)
            {
                ClientDependencyInclude ctlToRemove = null;
                if (!String.IsNullOrEmpty(Name))
                {
                    foreach (ClientDependencyInclude ctl in loader.Controls)
                    {
                        if (ctl.Name.ToLower() == Name && ctl.DependencyType == DependencyType)
                        {
                            ctlToRemove = ctl;
                            break;
                        }
                    }
                }
                if (ctlToRemove != null)
                {
                    loader.Controls.Remove(ctlToRemove);
                }
            }
        }
    }
}
