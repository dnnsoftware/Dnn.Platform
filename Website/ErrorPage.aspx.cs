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
using System.Web;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Services.FileSystem;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.Services.Exceptions
// ReSharper restore CheckNamespace
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : ErrorPage
    /// 
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Trapped errors are redirected to this universal error page, resulting in a 
    /// graceful display.
    /// </summary>
    /// <remarks>
    /// 'get the last server error
    /// 'process this error using the Exception Management Application Block
    /// 'add to a placeholder and place on page
    /// 'catch direct access - No exception was found...you shouldn't end up here unless you go to this aspx page URL directly
    /// </remarks>
    /// <history>
    /// 	[sun1]	1/19/2004	Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class ErrorPage : Page
    {
        private void ManageError(string status)
        {
            string errorMode = Config.GetCustomErrorMode();

            string errorMessage = HttpUtility.HtmlEncode(Request.QueryString["error"]);
            string errorMessage2 = HttpUtility.HtmlEncode(Request.QueryString["error2"]);
            string localizedMessage = Localization.Localization.GetString(status + ".Error", Localization.Localization.GlobalResourceFile);
            if (localizedMessage != null)
            {
                localizedMessage = localizedMessage.Replace("src=\"images/403-3.gif\"", "src=\"" + ResolveUrl("~/images/403-3.gif") + "\"");

                if (!string.IsNullOrEmpty(errorMessage2) && ( (errorMode=="Off") || ( (errorMode=="RemoteOnly") && (Request.IsLocal) ) ))
                {
                    ErrorPlaceHolder.Controls.Add(new LiteralControl(string.Format(localizedMessage, errorMessage2)));
                }
                else
                {
                    ErrorPlaceHolder.Controls.Add(new LiteralControl(string.Format(localizedMessage, errorMessage)));
                }
            }

            int statusCode;
            Int32.TryParse(status, out statusCode);

            if (statusCode > -1)
            {
                Response.StatusCode = statusCode;
            }
        }

        [Obsolete("Function obsoleted in 5.6.1 as no longer used in core - version identification can be useful to potential hackers if used incorrectly")]
        public string ExtractOSVersion()
        {
            //default name to OSVersion in case OS not recognised
            string commonName = Environment.OSVersion.ToString();
            switch (Environment.OSVersion.Version.Major)
            {
                case 5:
                    switch (Environment.OSVersion.Version.Minor)
                    {
                        case 0:
                            commonName = "Windows 2000";
                            break;
                        case 1:
                            commonName = "Windows XP";
                            break;
                        case 2:
                            commonName = "Windows Server 2003";
                            break;
                    }
                    break;
                case 6:
                    switch (Environment.OSVersion.Version.Minor)
                    {
                        case 0:
                            commonName = "Windows Vista";
                            break;
                        case 1:
                            commonName = "Windows Server 2008";
                            break;
                        case 2:
                            commonName = "Windows 7";
                            break;
                    }
                    break;
            }
            return commonName;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            DefaultStylesheet.Attributes["href"] = ResolveUrl("~/Portals/_default/default.css");
            InstallStylesheet.Attributes["href"] = ResolveUrl("~/Install/install.css");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings != null && !String.IsNullOrEmpty(portalSettings.LogoFile))
            {
                IFileInfo fileInfo = FileManager.Instance.GetFile(portalSettings.PortalId, portalSettings.LogoFile);
                if (fileInfo != null)
                {
                    headerImage.ImageUrl = FileManager.Instance.GetUrl(fileInfo);
                }
            }
            headerImage.Visible = !string.IsNullOrEmpty(headerImage.ImageUrl);

            string localizedMessage;
            var security = new PortalSecurity();
            string status = security.InputFilter(Request.QueryString["status"],
                                                    PortalSecurity.FilterFlag.NoScripting |
                                                    PortalSecurity.FilterFlag.NoMarkup);
            if (!string.IsNullOrEmpty(status))
                ManageError(status);
            else
            {
                //get the last server error
                var exc = Server.GetLastError();
                try
                {
                    if (Request.Url.LocalPath.ToLower().EndsWith("installwizard.aspx"))
                    {
                        ErrorPlaceHolder.Controls.Add(new LiteralControl(HttpUtility.HtmlEncode(exc.ToString())));
                    }
                    else
                    {
                        var lex = new PageLoadException(exc.Message, exc);
                        Exceptions.LogException(lex);
                        localizedMessage = Localization.Localization.GetString("Error.Text", Localization.Localization.GlobalResourceFile);
                        ErrorPlaceHolder.Controls.Add(new ErrorContainer(portalSettings, localizedMessage, lex).Container);
                    }
                }
                catch
                {
                    //No exception was found...you shouldn't end up here
                    //unless you go to this aspx page URL directly
                    localizedMessage = Localization.Localization.GetString("UnhandledError.Text", Localization.Localization.GlobalResourceFile);
                    ErrorPlaceHolder.Controls.Add(new LiteralControl(localizedMessage));
                }

                Response.StatusCode = 500;
            }
            localizedMessage = Localization.Localization.GetString("Return.Text", Localization.Localization.GlobalResourceFile);

            hypReturn.Text = localizedMessage;
        }
    }
}