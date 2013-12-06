#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.IO;
using System.Web.UI.WebControls;

using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.UserControls
{
    public abstract class Help : PortalModuleBase
    {
        private string MyFileName = "Help.ascx";
        private string _key;
        protected LinkButton cmdCancel;
        protected Literal helpFrame;
        protected Label lblHelp;
        protected Label lblInfo;
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            cmdCancel.Click += cmdCancel_Click;
            int moduleControlId = Null.NullInteger;

            if (Request.QueryString["ctlid"] != null)
            {
                moduleControlId = Int32.Parse(Request.QueryString["ctlid"]);
            }
            else if (Host.EnableModuleOnLineHelp)
            {
                helpFrame.Text = string.Format("<iframe src='{0}' id='helpFrame' width='100%' height='500' />", Host.HelpURL);
            }

            ModuleControlInfo objModuleControl = ModuleControlController.GetModuleControl(moduleControlId);
            if (objModuleControl != null)
            {
                if (!string.IsNullOrEmpty(objModuleControl.HelpURL) && Host.EnableModuleOnLineHelp)
                {
                    helpFrame.Text = string.Format("<iframe src='{0}' id='helpFrame' width='100%' height='500' />", objModuleControl.HelpURL); ;
                }
                else
                {
                    string fileName = Path.GetFileName(objModuleControl.ControlSrc);
                    string localResourceFile = objModuleControl.ControlSrc.Replace(fileName, Localization.LocalResourceDirectory + "/" + fileName);
                    if (!String.IsNullOrEmpty(Localization.GetString(ModuleActionType.HelpText, localResourceFile)))
                    {
                        lblHelp.Text = Localization.GetString(ModuleActionType.HelpText, localResourceFile);
                    }
                    else
                    {
                        lblHelp.Text = Localization.GetString("lblHelp.Text", Localization.GetResourceFile(this, MyFileName));
                    }
                }

                _key = objModuleControl.ControlKey;
                //display module info to Host users
                if (UserInfo.IsSuperUser)
                {
                    string strInfo = Localization.GetString("lblInfo.Text", Localization.GetResourceFile(this, MyFileName));
                    strInfo = strInfo.Replace("[CONTROL]", objModuleControl.ControlKey);
                    strInfo = strInfo.Replace("[SRC]", objModuleControl.ControlSrc);
                    ModuleDefinitionInfo objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(objModuleControl.ModuleDefID);
                    if (objModuleDefinition != null)
                    {
                        strInfo = strInfo.Replace("[DEFINITION]", objModuleDefinition.FriendlyName);
                        DesktopModuleInfo objDesktopModule = DesktopModuleController.GetDesktopModule(objModuleDefinition.DesktopModuleID, PortalId);
                        if (objDesktopModule != null)
                        {
                            PackageInfo objPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == objDesktopModule.PackageID);
                            if (objPackage != null)
                            {
                                strInfo = strInfo.Replace("[ORGANIZATION]", objPackage.Organization);
                                strInfo = strInfo.Replace("[OWNER]", objPackage.Owner);
                                strInfo = strInfo.Replace("[EMAIL]", objPackage.Email);
                                strInfo = strInfo.Replace("[URL]", objPackage.Url);
                                strInfo = strInfo.Replace("[MODULE]", objPackage.Name);
                                strInfo = strInfo.Replace("[VERSION]", objPackage.Version.ToString());
                            }
                        }
                    }
                    lblInfo.Text = Server.HtmlDecode(strInfo);
                }
            }
            if (Page.IsPostBack == false)
            {
                if (Request.UrlReferrer != null)
                {
                    ViewState["UrlReferrer"] = Convert.ToString(Request.UrlReferrer);
                }
                else
                {
                    ViewState["UrlReferrer"] = "";
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdCancel_Click runs when the cancel Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void cmdCancel_Click(Object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(Convert.ToString(ViewState["UrlReferrer"]), true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
