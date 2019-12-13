﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.ComponentModel;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotNetNuke.ExtensionPoints
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:EditPagePanelExtensionControl runat=server></{0}:EditPagePanelExtensionControl>")]
    public class EditPagePanelExtensionControl : DefaultExtensionControl
    {
        private void LoadControl(IEditPagePanelExtensionPoint extension)
        {
            var editPanel = new PanelEditPagePanelExtensionControl { PanelId = extension.EditPagePanelId, Text = extension.Text, CssClass = extension.CssClass };
            var control = Page.LoadControl(extension.UserControlSrc);
            control.ID = Path.GetFileNameWithoutExtension(extension.UserControlSrc);
            editPanel.Controls.Add(control);
            Controls.Add(editPanel);
            
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var extensionPointManager = new ExtensionPointManager();
            
            if (!String.IsNullOrEmpty(Name))
            {
                var extension = extensionPointManager.GetEditPagePanelExtensionPointFirstByPriority(Module, Name);
                if (extension != null)
                {
                    LoadControl(extension);                    
                }
            }
            else
            {
                foreach (var extension in extensionPointManager.GetEditPagePanelExtensionPoints(Module, Group))
                {
                    if (extension != null)
                    {
                        LoadControl(extension);                        
                    }
                }
            }
        }

        public void BindAction(int portalId, int tabId, int moduleId)
        {
            foreach (var control in Controls)
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
            foreach (var control in Controls)
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
            foreach (var control in Controls)
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
    }

    public class PanelEditPagePanelExtensionControl : WebControl
    {
        public string PanelId { get; set; }
        public string Text { get; set; }

        protected override void RenderContents(HtmlTextWriter op)
        {

            op.Write(@"<div class="""+CssClass+@""">
	<h2 id="""+PanelId+@""" class=""dnnFormSectionHead"">
<a href="" class=""dnnLabelExpanded"">"+Text+@"</a>
</h2>
	<fieldset>");
            base.RenderContents(op);
            op.Write("</fieldset></div>");
        }
    }
}
