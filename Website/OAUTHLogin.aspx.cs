using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Localization;
using OAuth.AuthorizationServer.Core.Utilities;

namespace DotNetNuke.Website
{
    public partial class OAUTHLogin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            cmdLogin.Click += cmdLogin_Click;
            if (Request.QueryString["failed"] != null)
            {
                failedMessage.Text = Localization.GetString("LoginFailed",
                    "~/desktopmodules/admin/authentication/app_localresources/Login.ascx.resx");
            }
        }

        private void cmdLogin_Click(object sender, EventArgs e)
        {
            PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings != null)
            {


                UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;
                UserInfo objUser = UserController.ValidateUser(portalSettings.PortalId,
                    txtUsername.Text,
                    txtPassword.Text,
                    "DNN",
                    "",
                    portalSettings.PortalName,
                    AuthenticationLoginBase.GetIPAddress(),
                    ref loginStatus);
                if (loginStatus == UserLoginStatus.LOGIN_SUCCESS || loginStatus == UserLoginStatus.LOGIN_SUPERUSER)
                {

                    string returnUrl = Request.QueryString["ReturnUrl"].ToString();
                    // No checking of password for this sample.  Just care about the username
                    // as that's what we're including in the token to send back to the authorization server

                    // Corresponds to shared secret the authorization server knows about for this resource
                    const string encryptionKey = "WebAPIsAreAwesome";

                    // Build token with info the authorization server needs to know
                    // var tokenContent = txtUsername.Text+ ";" + DateTime.Now.ToString(CultureInfo.InvariantCulture) + ";" + model.RememberMe;
                    var tokenContent = txtUsername.Text + ";" + DateTime.Now.ToString(CultureInfo.InvariantCulture) +
                                       ";" + "False";
                    var encryptedToken = EncodingUtility.Encode(tokenContent, encryptionKey);


                    returnUrl = Request.QueryString["ReturnUrl"].ToString() + "&scope=DNN-ALL&redirect_uri=" + Request.QueryString["redirect_uri"].ToString() + "&response_type=token";
                    if (returnUrl.Contains("client_id="))
                    {
                        returnUrl = returnUrl + "&resource-authentication-token=";
                    }
                    else
                    {
                        returnUrl = returnUrl + "&client_id=" + Request.QueryString["client_id"] + "&resource-authentication-token=";
                    }



                    //  returnUrl += "resource-authentication-token=" + encryptedToken;

                    returnUrl += encryptedToken;



                    var url = new Uri(returnUrl);


                    var redirectUrl = url.ToString();

                    // URL Encode the values of the querystring parameters
                    if (url.Query.Length > 1)
                    {
                        // var helper = new UrlHelper(HttpContext.Request.RequestContext);
                        //var helper = HttpUtility.UrlEncode(HttpContext.Request.RequestContext);
                        var helper = HttpUtility.UrlEncode(HttpContext.Current.Request.ToString());
                        var qsParts = HttpUtility.ParseQueryString(url.Query);
                        //redirectUrl = url.GetLeftPart(UriPartial.Path) + "?" + String.Join("&", qsParts.AllKeys.Select(x => x + "=" + helper.Encode(qsParts[x])));
                        redirectUrl = url.GetLeftPart(UriPartial.Path) + "?" +
                                      String.Join("&",
                                          qsParts.AllKeys.Select(x => x + "=" + HttpUtility.UrlEncode(qsParts[x])));
                    }

                    //return Redirect(redirectUrl);
                    Response.Redirect(redirectUrl);
                }
                else
                {
                    Response.Redirect(Request.RawUrl +"&failed=true");
                }
            }

        }
    }
}