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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.UserControls;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace DotNetNuke.Web.UI.WebControls
{
	public class DnnFormLabel : Panel
	{
		public string AssociatedControlID { get; set; }

		public string LocalResourceFile { get; set; }

		public string ResourceKey { get; set; }

		public string ToolTipKey { get; set; }

        public bool RequiredField { get; set; }

		protected override void CreateChildControls()
		{
			string toolTipText = LocalizeString(ToolTipKey);
            if (string.IsNullOrEmpty(CssClass))
                CssClass = "dnnLabel";

            else if (!CssClass.Contains("dnnLabel"))                           
                CssClass += " dnnLabel";
            

            //var outerPanel = new Panel();
            //outerPanel.CssClass = "dnnLabel";
            //Controls.Add(outerPanel);            

            var outerLabel = new System.Web.UI.HtmlControls.HtmlGenericControl { TagName = "label" };
            Controls.Add(outerLabel);
            
            var label = new Label { ID = "Label", Text = LocalizeString(ResourceKey) };
            if (RequiredField)
            {
                label.CssClass += " dnnFormRequired";
            }
		    outerLabel.Controls.Add(label);			

            var link = new LinkButton { ID = "Link", CssClass = "dnnFormHelp", TabIndex = -1 };
            link.Attributes.Add("aria-label", "Help");
            Controls.Add(link);
			
			if (!String.IsNullOrEmpty(toolTipText))
			{
				//CssClass += "dnnLabel";

			    var tooltipPanel = new Panel() { CssClass = "dnnTooltip"};
                Controls.Add(tooltipPanel);

				var panel = new Panel { ID = "Help", CssClass = "dnnFormHelpContent dnnClear" };				
                tooltipPanel.Controls.Add(panel);
				
				var helpLabel = new Label { ID = "Text", CssClass="dnnHelpText", Text = LocalizeString(ToolTipKey) };
				panel.Controls.Add(helpLabel);

				var pinLink = new HyperLink {CssClass = "pinHelp"};
			    pinLink.Attributes.Add("href", "#");
				panel.Controls.Add(pinLink);

                JavaScript.RegisterClientReference(Page, ClientAPI.ClientNamespaceReferences.dnn);
                JavaScript.RequestRegistration(CommonJs.DnnPlugins);
                //ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Scripts/initTooltips.js");
			}
		}

		protected string LocalizeString(string key)
		{
			return Localization.GetString(key, LocalResourceFile);
		}
	}
}
