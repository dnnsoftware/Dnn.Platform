// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Common.Controls
{
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// The Privacy PortalModuleBase displays the Privacy text
	/// </summary>
	/// <returns></returns>
	/// <remarks>
	/// </remarks>
	/// -----------------------------------------------------------------------------
    public partial class Privacy : PortalModuleBase
    {
		#region " Web Form Designer Generated Code "
		
        private void InitializeComponent()
        {
        }
		
		#endregion

		#region "Event Handlers"

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //CODEGEN: This method call is required by the Web Form Designer
			//Do not modify it using the code editor.
			InitializeComponent();
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Page_Load runs when the control is loaded.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// </remarks>
		/// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!Page.IsPostBack)
                {
                    lblPrivacy.Text = Localization.GetSystemMessage(PortalSettings, "MESSAGE_PORTAL_PRIVACY");
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
		
		#endregion
    }
}
