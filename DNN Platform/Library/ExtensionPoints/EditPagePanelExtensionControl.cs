#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
