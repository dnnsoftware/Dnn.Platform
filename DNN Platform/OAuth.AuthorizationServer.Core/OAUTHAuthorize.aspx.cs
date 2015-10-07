using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using OAuth.AuthorizationServer.Core.Server;

/// <summary>
/// page used to authorize oauth requests
/// </summary>
public partial class OAUTHAuthorize : System.Web.UI.Page
{
    /// <summary>
    /// initialize page
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void Page_Init(object sender, EventArgs e)
    {
        bool isAllowed = true;
        try
        {
            if (Host.EnableOAuthAuthorization == false)
            {
                isAllowed = false;
            }

            PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings == null)
            {
                isAllowed = false;
            }
            else
            {
                var portalOAuth = PortalController.GetPortalSettingAsBoolean("EnableOAuthAuthorization",
                    portalSettings.PortalId, false);
                if (portalOAuth == false)
                {
                    isAllowed = false;
                }

            }
        }
        catch (Exception)
        {

            isAllowed = false;
        }



        if (isAllowed == false)
        {
            Response.Redirect(Globals.NavigateURL(PortalController.Instance.GetCurrentPortalSettings().ErrorPage404, string.Empty, "status=404"));
        }

    }

   
}