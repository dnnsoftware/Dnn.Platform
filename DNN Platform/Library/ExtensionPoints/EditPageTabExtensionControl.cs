#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace DotNetNuke.ExtensionPoints
{
    [DefaultProperty("Module")]
    [ToolboxData("<{0}:EditPageTabExtensionControl runat=server></{0}:EditPageTabExtensionControl>")]
    public class EditPageTabExtensionControl : DefaultExtensionControl
    {
        [Bindable(true)]
        [DefaultValue("")]
        public string TabControlId
        {
            get
            {
                var s = (String)ViewState["TabControlId"];
                return (s ?? String.Empty);
            }
            set
            {
                ViewState["TabControlId"] = value;
            }
        }

        [Bindable(true)]
        [DefaultValue("")]
        public string PanelControlId
        {
            get
            {
                var s = (String)ViewState["PanelControlId"];
                return (s ?? String.Empty);
            }
            set
            {
                ViewState["PanelControlId"] = value;
            }
        }
        
        protected override void OnInit(EventArgs e)
        {
            var extensionPointManager = new ExtensionPointManager();
            
            var tabs = (HtmlGenericControl)Parent.FindControl(TabControlId);
            var panel = Parent.FindControl(PanelControlId);

            foreach (var extension in extensionPointManager.GetEditPageTabExtensionPoints(Module, Group))
            {
                if (extension.Visible)
                {
                    var liElement = new HtmlGenericControl("li")
                    {
                        InnerHtml = "<a href=\"#" + extension.EditPageTabId + "\">" + extension.Text + "</a>",
                    };
                    liElement.Attributes.Add("class", extension.CssClass);
                    tabs.Controls.Add(liElement);

                    var container = new PanelTabExtensionControl { PanelId = extension.EditPageTabId };
                    var control = Page.LoadControl(extension.UserControlSrc);
                    control.ID = Path.GetFileNameWithoutExtension(extension.UserControlSrc);
                    container.Controls.Add(control);
                    panel.Controls.Add(container);
                }
            }
        }

        public void BindAction(int portalId, int tabId, int moduleId)
        {
            var panel = Parent.FindControl(PanelControlId);

            foreach (var control in panel.Controls)
            {
                var panelcontrol = control as PanelTabExtensionControl;
                if (panelcontrol != null)
                {
                    foreach (var extensionControl in panelcontrol.Controls)
                    {
                        var actionsControl = extensionControl as IEditPageTabControlActions;
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
            var panel = Parent.FindControl(PanelControlId);

            foreach (var control in panel.Controls)
            {
                var panelcontrol = control as PanelTabExtensionControl;
                if (panelcontrol != null)
                {
                    foreach (var extensionControl in panelcontrol.Controls)
                    {
                        var actionsControl = extensionControl as IEditPageTabControlActions;
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
            var panel = Parent.FindControl(PanelControlId);

            foreach (var control in panel.Controls)
            {
                var panelcontrol = control as PanelTabExtensionControl;
                if (panelcontrol != null)
                {
                    foreach (var extensionControl in panelcontrol.Controls)
                    {
                        var actionsControl = extensionControl as IEditPageTabControlActions;
                        if (actionsControl != null)
                        {
                            actionsControl.CancelAction(portalId, tabId, moduleId);
                        }
                    }
                }
            }
        }

    }

    public class PanelTabExtensionControl : WebControl
    {
        public string PanelId { get; set; }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.Write("");
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.Write("");
        } 

        protected override void RenderContents(HtmlTextWriter op)
        {
            op.Write("<div class=\"ehccContent dnnClear\" id=\"" + PanelId + "\">");
            base.RenderContents(op);
            op.Write("</div>");
        } 
    }
}
