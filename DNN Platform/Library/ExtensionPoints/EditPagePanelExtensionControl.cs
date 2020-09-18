// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [DefaultProperty("Text")]
    [ToolboxData("<{0}:EditPagePanelExtensionControl runat=server></{0}:EditPagePanelExtensionControl>")]
    public class EditPagePanelExtensionControl : DefaultExtensionControl
    {
        public void BindAction(int portalId, int tabId, int moduleId)
        {
            foreach (var control in this.Controls)
            {
                var panelcontrol = control as PanelEditPagePanelExtensionControl;
                if (panelcontrol != null)
                {
                    foreach (var extensionControl in panelcontrol.Controls)
                    {
                        var actionsControl = extensionControl as IEditPagePanelControlActions;
                        if (actionsControl != null)
                        {
                            actionsControl.BindAction(portalId, tabId, moduleId);
                        }
                    }
                }
            }
        }

        public void SaveAction(int portalId, int tabId, int moduleId)
        {
            foreach (var control in this.Controls)
            {
                var panelcontrol = control as PanelEditPagePanelExtensionControl;
                if (panelcontrol != null)
                {
                    foreach (var extensionControl in panelcontrol.Controls)
                    {
                        var actionsControl = extensionControl as IEditPagePanelControlActions;
                        if (actionsControl != null)
                        {
                            actionsControl.SaveAction(portalId, tabId, moduleId);
                        }
                    }
                }
            }
        }

        public void CancelAction(int portalId, int tabId, int moduleId)
        {
            foreach (var control in this.Controls)
            {
                var panelcontrol = control as PanelEditPagePanelExtensionControl;
                if (panelcontrol != null)
                {
                    foreach (var extensionControl in panelcontrol.Controls)
                    {
                        var actionsControl = extensionControl as IEditPagePanelControlActions;
                        if (actionsControl != null)
                        {
                            actionsControl.CancelAction(portalId, tabId, moduleId);
                        }
                    }
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var extensionPointManager = new ExtensionPointManager();

            if (!string.IsNullOrEmpty(this.Name))
            {
                var extension = extensionPointManager.GetEditPagePanelExtensionPointFirstByPriority(this.Module, this.Name);
                if (extension != null)
                {
                    this.LoadControl(extension);
                }
            }
            else
            {
                foreach (var extension in extensionPointManager.GetEditPagePanelExtensionPoints(this.Module, this.Group))
                {
                    if (extension != null)
                    {
                        this.LoadControl(extension);
                    }
                }
            }
        }

        private void LoadControl(IEditPagePanelExtensionPoint extension)
        {
            var editPanel = new PanelEditPagePanelExtensionControl { PanelId = extension.EditPagePanelId, Text = extension.Text, CssClass = extension.CssClass };
            var control = this.Page.LoadControl(extension.UserControlSrc);
            control.ID = Path.GetFileNameWithoutExtension(extension.UserControlSrc);
            editPanel.Controls.Add(control);
            this.Controls.Add(editPanel);
        }
    }

    public class PanelEditPagePanelExtensionControl : WebControl
    {
        public string PanelId { get; set; }

        public string Text { get; set; }

        protected override void RenderContents(HtmlTextWriter op)
        {
            op.Write(@"<div class=""" + this.CssClass + @""">
	<h2 id=""" + this.PanelId + @""" class=""dnnFormSectionHead"">
<a href="" class=""dnnLabelExpanded"">" + this.Text + @"</a>
</h2>
	<fieldset>");
            base.RenderContents(op);
            op.Write("</fieldset></div>");
        }
    }
}
