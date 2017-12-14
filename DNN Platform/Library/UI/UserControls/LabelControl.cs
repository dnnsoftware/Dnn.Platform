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
#region Usings

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;

#endregion
				
namespace DotNetNuke.UI.UserControls
{

	/// <summary>
	/// LabelControl is a user control that provides all the server code to manage a
	/// label, including localization, 508 support and help.
	/// </summary>
	/// <remarks>
	/// To implement help, the control uses the ClientAPI interface.  In particular
	///  the javascript function __dnn_Help_OnClick()
	/// </remarks>
	public abstract class LabelControl : UserControl
	{

		#region Private Memebers
				
		protected LinkButton cmdHelp;
		protected HtmlGenericControl label;
		protected Label lblHelp;
		protected Label lblLabel;
		protected Panel pnlHelp;
		protected Label lblNoHelpLabel;

		#endregion
		
		#region Properties
																									 
		/// <summary>
		/// ControlName is the Id of the control that is associated with the label
		/// </summary>
		/// <value>A string representing the id of the associated control</value>
		/// <remarks>
		/// </remarks>
		public string ControlName { get; set; }

		/// <summary>
		/// Set the associate control id format, combined used with ControlName for controls
		///  which have child edit control and want that child control focus when click label.
		/// </summary>
		public string AssociateFormat { get; set; }

		/// <summary>
		/// Css style applied to the asp:label control
		/// </summary>
		/// <value>A string representing css class name</value>
		/// <remarks>
		/// </remarks>
		public string CssClass { get; set; }

		/// <summary>
		/// HelpKey is the Resource Key for the Help Text
		/// </summary>
		/// <value>A string representing the Resource Key for the Help Text</value>
		/// <remarks>
		/// </remarks>
		public string HelpKey { get; set; }

		/// <summary>
		/// HelpText is value of the Help Text if no ResourceKey is provided
		/// </summary>
		/// <value>A string representing the Text</value>
		/// <remarks>
		/// </remarks>                                   
		public string HelpText
		{
			get
			{
				return lblHelp.Text;
			}
			set
			{
				lblHelp.Text = value;
			}
		}

		public string NoHelpLabelText
		{
			get
			{
				return lblNoHelpLabel.Text;
			}
			set
			{
				lblNoHelpLabel.Text = value;
			}
		}

		/// <summary>
		/// ResourceKey is the Resource Key for the Label Text
		/// </summary>
		/// <value>A string representing the Resource Key for the Label Text</value>
		/// <remarks>
		/// </remarks>
		public string ResourceKey { get; set; }

		/// <summary>
		/// Suffix is Optional Text that appears after the Localized Label Text
		/// </summary>
		/// <value>A string representing the Optional Text</value>
		/// <remarks>
		/// </remarks>
		public string Suffix { get; set; }

		/// <summary>
		/// Text is value of the Label Text if no ResourceKey is provided
		/// </summary>
		/// <value>A string representing the Text</value>
		/// <remarks>
		/// </remarks>
		public string Text
		{
			get
			{
				return lblLabel.Text;
			}
			set
			{
				lblLabel.Text = value;
			}
		}

		/// <summary>
		/// Width is value of the Label Width
		/// </summary>
		/// <value>A string representing the Text</value>
		/// <remarks>
		/// </remarks>
		public Unit Width
		{
			get
			{
				return lblLabel.Width;
			}
			set
			{
				lblLabel.Width = value;
			}
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Page_Load runs when the control is loaded
		/// </summary>
		/// <remarks>
		/// </remarks>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			try
			{
				RegisterClientDependencies();
			}
			catch (Exception exc) //Module failed to load
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			//get the localised text
			if (String.IsNullOrEmpty(ResourceKey))
			{
				//Set Resource Key to the ID of the control
				ResourceKey = ID;
			}
			if ((!string.IsNullOrEmpty(ResourceKey)))
			{
				var localText = Localization.GetString(ResourceKey, this);
				if (!string.IsNullOrEmpty(localText))
				{
					Text = localText + Suffix;
					//NoHelpLabelText = Text;
				}
				else
				{
					Text += Suffix;
					//NoHelpLabelText = Text;
				}
			}

			//Set Help Key to the Resource Key plus ".Help"
			if (String.IsNullOrEmpty(HelpKey))
			{
				HelpKey = ResourceKey + ".Help";
			}

			var helpText = Localization.GetString(HelpKey, this);
			if ((!string.IsNullOrEmpty(helpText)) || (string.IsNullOrEmpty(HelpText)))
			{
				HelpText = helpText;
			}

			if (string.IsNullOrEmpty(HelpText))
			{
				pnlHelp.Visible = cmdHelp.Visible = false;
                //lblHelp.Visible = false;
                //lblNoHelpLabel.Visible = true;
			}

			if (!string.IsNullOrEmpty(CssClass))
			{
				lblLabel.CssClass = CssClass;
			}

			//find the reference control in the parents Controls collection
			if (!String.IsNullOrEmpty(ControlName))
			{
				var c = Parent.FindControl(ControlName);
			    var clientId = ControlName;
				if (c != null)
				{
					clientId = c.ClientID;
				}

                if (!string.IsNullOrEmpty(AssociateFormat))
                {
                    clientId = string.Format(AssociateFormat, clientId);
                }
                label.Attributes["for"] = clientId;
            }
		}

		private void RegisterClientDependencies()
		{
            JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            //ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Scripts/initTooltips.js");
		}

		#endregion

	}
}
