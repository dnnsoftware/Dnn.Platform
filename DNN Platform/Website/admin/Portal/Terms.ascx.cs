// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Common.Controls
{
    public partial class Terms : PortalModuleBase
    {
		#region " Web Form Designer Generated Code "

        //This call is required by the Web Form Designer.
        private void InitializeComponent()
        {
        }
		
		#endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            
			//CODEGEN: This method call is required by the Web Form Designer
            //Do not modify it using the code editor.
			InitializeComponent();
        }

        /// <summary>The Page_Load server event handler on this page is used to populate the role information for the page</summary>
		protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!Page.IsPostBack)
                {
                    lblTerms.Text = Localization.GetSystemMessage(PortalSettings, "MESSAGE_PORTAL_TERMS");
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
