﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.IO;
using System.Threading;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Modules.Admin.Authentication
{

	/// <summary>
	/// The Logoff UserModuleBase is used to log off a registered user
	/// </summary>
	/// <remarks>
	/// </remarks>
	public partial class Logoff : UserModuleBase
	{

		#region Private Methods

		private void Redirect()
		{
			//Redirect browser back to portal 
			this.Response.Redirect(AuthenticationController.GetLogoffRedirectURL(this.PortalSettings, this.Request), true);
		}

		private void DoLogoff()
		{
			try
			{
				//Remove user from cache
				if (this.User != null)
				{
					DataCache.ClearUserCache(this.PortalSettings.PortalId, this.Context.User.Identity.Name);
				}
				var objPortalSecurity = PortalSecurity.Instance;
				objPortalSecurity.SignOut();
			}
			catch (Exception exc)	//Page failed to load
			{
				Exceptions.ProcessPageLoadException(exc);
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
				//Get the Authentication System associated with the current User
				var authSystem = AuthenticationController.GetAuthenticationType();

				if (authSystem != null && !string.IsNullOrEmpty(authSystem.LogoffControlSrc))
				{
					var authLogoffControl = (AuthenticationLogoffBase) this.LoadControl("~/" + authSystem.LogoffControlSrc);

					//set the control ID to the resource file name ( ie. controlname.ascx = controlname )
					//this is necessary for the Localization in PageBase
					authLogoffControl.AuthenticationType = authSystem.AuthenticationType;
					authLogoffControl.ID = Path.GetFileNameWithoutExtension(authSystem.LogoffControlSrc) + "_" + authSystem.AuthenticationType;
					authLogoffControl.LocalResourceFile = authLogoffControl.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" +
														  Path.GetFileNameWithoutExtension(authSystem.LogoffControlSrc);
					authLogoffControl.ModuleConfiguration = this.ModuleConfiguration;

					authLogoffControl.LogOff += this.UserLogOff;
					authLogoffControl.Redirect += this.UserRedirect;

					//Add Login Control to Control
					this.pnlLogoffContainer.Controls.Add(authLogoffControl);
				}
				else
				{
					//The current auth system has no custom logoff control so LogOff
					this.DoLogoff();
					this.Redirect();
				}
			}
			catch (ThreadAbortException)
			{
				//Do nothing Response.redirect
			}
			catch (Exception exc) //Page failed to load
			{
				Exceptions.ProcessPageLoadException(exc);
			}
		}

		protected void UserLogOff(Object sender, EventArgs e)
		{
			this.DoLogoff();
		}

		protected void UserRedirect(Object sender, EventArgs e)
		{
			this.Redirect();
		}

		#endregion

	}
}
