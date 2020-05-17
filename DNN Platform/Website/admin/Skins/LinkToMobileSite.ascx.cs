// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mobile;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary>Skin object of portal links between desktop and mobile portals.</summary>
    /// <returns></returns>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class LinkToMobileSite : SkinObjectBase
    {
        private const string MyFileName = "LinkToMobileSite.ascx";

    	private string _localResourcesFile;

    	private string LocalResourcesFile
    	{
    		get
    		{
    			if(string.IsNullOrEmpty(_localResourcesFile))
    			{
    				_localResourcesFile = Localization.GetResourceFile(this, MyFileName);
    			}

    			return _localResourcesFile;
    		}
    	}

        #region "Event Handlers"
        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            var redirectionController = new RedirectionController();
            var redirectUrl = redirectionController.GetMobileSiteUrl();
            if (!string.IsNullOrEmpty(redirectUrl))
            {                
                lnkPortal.NavigateUrl = redirectUrl;                
                lnkPortal.Text = Localization.GetString("lnkPortal", LocalResourcesFile);
            }
            else
            {
                this.Visible = false;                    
            }
        }


        #endregion
    }
}
