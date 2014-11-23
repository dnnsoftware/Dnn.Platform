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
	/// <history>
	/// 	[cnurse]	5/10/2004	Updated to reflect design changes for Help, 508 support
	///                       and localisation
	/// </history>
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
		/// <history>
		/// 	[cnurse]	10/5/2004	Updated to reflect design changes for Help, 508 support
		///                       and localisation
		/// </history>
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