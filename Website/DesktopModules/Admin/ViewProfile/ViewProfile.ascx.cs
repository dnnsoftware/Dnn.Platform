#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Users
{

	/// <summary>
	///   The ViewProfile ProfileModuleUserControlBase is used to view a Users Profile
	/// </summary>
    public partial class ViewProfile : ProfileModuleUserControlBase
	{
		public override bool DisplayModule
		{
			get
			{
				return true;
			}
		}

        public bool IncludeButton   
        {
            get
            {
                var includeButton = true;
                if (ModuleContext.Settings.ContainsKey("IncludeButton"))
                {
                    includeButton = Convert.ToBoolean(ModuleContext.Settings["IncludeButton"]);
                }
                return includeButton;
            }
        }

        public string ProfileProperties { get; set; }

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

			//redirect to home page if the user has been deleted
			if(ProfileUser == null || ProfileUser.IsDeleted)
			{
				Response.Redirect(GetRedirectUrl(), true);
			}

            jQuery.RegisterJQuery(Page);
        }

		/// <summary>
		///   Page_Load runs when the control is loaded
		/// </summary>
		/// <remarks>
		/// </remarks>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
                if(Null.IsNull(ProfileUserId))
                {
                    Visible = false;
                    return;
                }

                var template = Convert.ToString(ModuleContext.Settings["ProfileTemplate"]);
                if(string.IsNullOrEmpty(template))
                {
                    template = Localization.GetString("DefaultTemplate", LocalResourceFile);
                }
			    var editUrl = Globals.NavigateURL(ModuleContext.PortalSettings.ActiveTab.TabID, "Profile", "userId=" + ProfileUserId, "pageno=1");
                var profileUrl = Globals.NavigateURL(ModuleContext.PortalSettings.ActiveTab.TabID, "Profile", "userId=" + ProfileUserId, "pageno=2");

                if (template.Contains("[BUTTON:EDITPROFILE]"))
                {
                    if (IncludeButton && IsUser)
                    {
                        string editHyperLink = String.Format("<a href=\"{0}\" class=\"dnnPrimaryAction\">{1}</a>", profileUrl, LocalizeString("Edit"));
                        template = template.Replace("[BUTTON:EDITPROFILE]", editHyperLink);
                    }
                    buttonPanel.Visible = false;
                }
                else
                {
                    buttonPanel.Visible = IncludeButton;
                    editLink.NavigateUrl = editUrl;
                }
                if (template.Contains("[HYPERLINK:EDITPROFILE]"))
                {
                    if (IsUser)
                    {
                        string editHyperLink = String.Format("<a href=\"{0}\" class=\"dnnSecondaryAction\">{1}</a>", profileUrl, LocalizeString("Edit"));
                        template = template.Replace("[HYPERLINK:EDITPROFILE]", editHyperLink);
                    }
                }
                if (template.Contains("[HYPERLINK:MYACCOUNT]"))
                {
                    if (IsUser)
                    {
                        string editHyperLink = String.Format("<a href=\"{0}\" class=\"dnnSecondaryAction\">{1}</a>", editUrl, LocalizeString("MyAccount"));
                        template = template.Replace("[HYPERLINK:MYACCOUNT]", editHyperLink);
                    }
                    buttonPanel.Visible = false;
                }

                if (!IsUser && buttonPanel.Visible)
                {
                    buttonPanel.Visible = false;
                }

			    if (ProfileUser.Profile.ProfileProperties.Cast<ProfilePropertyDefinition>().Count(profProperty => profProperty.Visible) == 0)
                {
                    noPropertiesLabel.Visible = true;
                    profileOutput.Visible = false;
                }
                else
                {
                    var token = new TokenReplace { User = ProfileUser, AccessingUser = ModuleContext.PortalSettings.UserInfo };
                    profileOutput.InnerHtml = token.ReplaceEnvironmentTokens(template);
                    noPropertiesLabel.Visible = false;
                    profileOutput.Visible = true;
                }

			    var propertyAccess = new ProfilePropertyAccess(ProfileUser);
                var profileResourceFile = "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx";
                StringBuilder sb = new StringBuilder();
                bool propertyNotFound = false;

                foreach (ProfilePropertyDefinition property in ProfileUser.Profile.ProfileProperties)
                {
                    string value = propertyAccess.GetProperty(property.PropertyName,
                                                              String.Empty,
                                                              Thread.CurrentThread.CurrentUICulture,
                                                              ModuleContext.PortalSettings.UserInfo,
                                                              Scope.DefaultSettings,
                                                              ref propertyNotFound);


                    var clientName = Localization.GetSafeJSString(property.PropertyName);
                    sb.Append("self['" + clientName + "'] = ko.observable(");
                    sb.Append("\"");
                    value = Localization.GetSafeJSString(Server.HtmlDecode(value));
                    value = value.Replace("\r", string.Empty).Replace("\n",  " ");
                    sb.Append(value + "\"" + ");");
                    sb.Append('\n');
                    sb.Append("self['" + clientName + "Text'] = '");
                    sb.Append(clientName + "';");
                    sb.Append('\n');
                }

			    string email = (ProfileUserId == ModuleContext.PortalSettings.UserId
			                    || ModuleContext.PortalSettings.UserInfo.IsInRole(ModuleContext.PortalSettings.AdministratorRoleName))
			                       ? ProfileUser.Email
			                       : String.Empty;

                sb.Append("self.Email = ko.observable('");
                sb.Append(email + "');");
                sb.Append('\n');
                sb.Append("self.EmailText = '");
                sb.Append(LocalizeString("Email") + "';");
                sb.Append('\n');


                ProfileProperties = sb.ToString();


			}
			catch (Exception exc)
			{
				//Module failed to load
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		#endregion

		#region Private Methods

		private string GetRedirectUrl()
		{
			//redirect user to default page if not specific the home tab, do this action to prevent loop redirect.
			var homeTabId = ModuleContext.PortalSettings.HomeTabId;
			string redirectUrl;

			if (homeTabId > Null.NullInteger)
			{
				redirectUrl = Globals.NavigateURL(homeTabId);
			}
			else
			{
				redirectUrl = Globals.GetPortalDomainName(PortalSettings.Current.PortalAlias.HTTPAlias, Request, true) +
							  "/" + Globals.glbDefaultPage;
			}

			return redirectUrl;
		}

		#endregion
	}
}