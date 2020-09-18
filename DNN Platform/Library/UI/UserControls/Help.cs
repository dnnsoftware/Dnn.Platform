// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
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

    public abstract class Help : PortalModuleBase
    {
        protected LinkButton cmdCancel;
        protected HyperLink cmdHelp;
        protected Literal helpFrame;
        protected Label lblHelp;
        protected Label lblInfo;
        private string MyFileName = "Help.ascx";
        private string _key;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.cmdCancel.Click += this.cmdCancel_Click;
            int moduleControlId = Null.NullInteger;

            if (this.Request.QueryString["ctlid"] != null)
            {
                moduleControlId = int.Parse(this.Request.QueryString["ctlid"]);
            }
            else if (Host.EnableModuleOnLineHelp)
            {
                this.helpFrame.Text = string.Format("<iframe src='{0}' id='helpFrame' width='100%' height='500'></iframe>", Host.HelpURL);
            }

            ModuleControlInfo objModuleControl = ModuleControlController.GetModuleControl(moduleControlId);
            if (objModuleControl != null)
            {
                if (!string.IsNullOrEmpty(objModuleControl.HelpURL) && Host.EnableModuleOnLineHelp)
                {
                    this.helpFrame.Text = string.Format("<iframe src='{0}' id='helpFrame' width='100%' height='500'></iframe>", objModuleControl.HelpURL);
                }
                else
                {
                    string fileName = Path.GetFileName(objModuleControl.ControlSrc);
                    string localResourceFile = objModuleControl.ControlSrc.Replace(fileName, Localization.LocalResourceDirectory + "/" + fileName);
                    if (!string.IsNullOrEmpty(Localization.GetString(ModuleActionType.HelpText, localResourceFile)))
                    {
                        this.lblHelp.Text = Localization.GetString(ModuleActionType.HelpText, localResourceFile);
                    }
                    else
                    {
                        this.lblHelp.Text = Localization.GetString("lblHelp.Text", Localization.GetResourceFile(this, this.MyFileName));
                    }
                }

                this._key = objModuleControl.ControlKey;

                // display module info to Host users
                if (this.UserInfo.IsSuperUser)
                {
                    string strInfo = Localization.GetString("lblInfo.Text", Localization.GetResourceFile(this, this.MyFileName));
                    strInfo = strInfo.Replace("[CONTROL]", objModuleControl.ControlKey);
                    strInfo = strInfo.Replace("[SRC]", objModuleControl.ControlSrc);
                    ModuleDefinitionInfo objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(objModuleControl.ModuleDefID);
                    if (objModuleDefinition != null)
                    {
                        strInfo = strInfo.Replace("[DEFINITION]", objModuleDefinition.FriendlyName);
                        DesktopModuleInfo objDesktopModule = DesktopModuleController.GetDesktopModule(objModuleDefinition.DesktopModuleID, this.PortalId);
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

                    this.lblInfo.Text = this.Server.HtmlDecode(strInfo);
                }

                this.cmdHelp.Visible = !string.IsNullOrEmpty(objModuleControl.HelpURL);
            }

            if (this.Page.IsPostBack == false)
            {
                if (this.Request.UrlReferrer != null)
                {
                    this.ViewState["UrlReferrer"] = Convert.ToString(this.Request.UrlReferrer);
                }
                else
                {
                    this.ViewState["UrlReferrer"] = string.Empty;
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdCancel_Click runs when the cancel Button is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.Response.Redirect(Convert.ToString(this.ViewState["UrlReferrer"]), true);
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
