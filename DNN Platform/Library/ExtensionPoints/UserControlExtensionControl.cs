using System;
using System.ComponentModel;
using System.IO;
using System.Web.UI;

namespace DotNetNuke.ExtensionPoints
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:UserControlExtensionControl runat=server></{0}:UserControlExtensionControl>")]
    public class UserControlExtensionControl : DefaultExtensionControl
    {
        private void LoadControl(IUserControlExtensionPoint extension)
        {
            var control = Page.LoadControl(extension.UserControlSrc);
            control.ID = Path.GetFileNameWithoutExtension(extension.UserControlSrc);
            Controls.Add(control);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var extensionPointManager = new ExtensionPointManager();
            
            if (!String.IsNullOrEmpty(Name))
            {
                var extension = extensionPointManager.GetUserControlExtensionPointFirstByPriority(Module, Name);
                LoadControl(extension);
            }
            else
            {
                foreach (var extension in extensionPointManager.GetUserControlExtensionPoints(Module, Group))
                {
                    LoadControl(extension);
                }
            }
        }

        public void BindAction(int portalId, int tabId, int moduleId)
        {
            foreach (var control in Controls)
            {
                var actionsControl = control as IUserControlActions;
                if (actionsControl != null)
                {
                    actionsControl.BindAction(portalId, tabId, moduleId);
                }
            }
        }

        public void SaveAction(int portalId, int tabId, int moduleId)
        {
            foreach (var control in Controls)
            {
                var actionsControl = control as IUserControlActions;
                if (actionsControl != null)
                {
                    actionsControl.SaveAction(portalId, tabId, moduleId);
                }
            }
        }

        public void CancelAction(int portalId, int tabId, int moduleId)
        {
            foreach (var control in Controls)
            {
                var actionsControl = control as IUserControlActions;
                if (actionsControl != null)
                {
                    actionsControl.CancelAction(portalId, tabId, moduleId);
                }
            }
        }
    }
}