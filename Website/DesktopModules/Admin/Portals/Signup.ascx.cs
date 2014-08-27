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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.UI.WebControls;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Mail;
using DotNetNuke.UI.Skins.Controls;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Modules.Admin.Portals
{
    public partial class Signup : PortalModuleBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (Signup));

        #region Private Properties

        private CultureDropDownTypes DisplayType { get; set; }

        #endregion
        
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdCancel.Click += cmdCancel_Click;
            cmdUpdate.Click += cmdUpdate_Click;
            optType.SelectedIndexChanged += optType_SelectedIndexChanged;
            btnCustomizeHomeDir.Click += btnCustomizeHomeDir_Click;
            cboTemplate.SelectedIndexChanged += cboTemplate_SelectedIndexChanged;
            useCurrent.CheckedChanged += useCurrent_CheckedChanged;

            //Customise the Control Title
            if (IsHostMenu)
            {
                ModuleConfiguration.ModuleTitle = Localization.GetString("AddPortal", LocalResourceFile);
            }

            jQuery.RequestDnnPluginsRegistration();
        }

        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
 
            try
            {
                //ensure portal signup is allowed
                if ((!IsHostMenu || UserInfo.IsSuperUser == false) && !Host.DemoSignup)
                {
                    Response.Redirect(Globals.NavigateURL("Access Denied"), true);
                }
                valEmail2.ValidationExpression = Globals.glbEmailRegEx;

                //set the async timeout same with script time out
                AJAX.GetScriptManager(Page).AsyncPostBackTimeout = 900;

                if (!Page.IsPostBack)
                {
                    BindTemplates();
                    BindSiteGroups();
                    // load template description
                    cboTemplate_SelectedIndexChanged(null, null);

                    if (UserInfo.IsSuperUser)
                    {
                        rowType.Visible = true;
                        useCurrentPanel.Visible = true;
                        useCurrent.Checked = true;
                        adminUserPanel.Visible = false;

                        optType.SelectedValue = "P";
                    }
                    else
                    {
                        useCurrentPanel.Visible = false;
                        useCurrent.Checked = false;
                        adminUserPanel.Visible = true;

                        optType.SelectedValue = "C";

						txtPortalAlias.Text = Globals.GetDomainName(Request) + @"/";
                        rowType.Visible = false;
                        string strMessage = string.Format(Localization.GetString("DemoMessage", LocalResourceFile),
                                                          Host.DemoPeriod != Null.NullInteger ? " for " + Host.DemoPeriod + " days" : "",
                                                          Globals.GetDomainName(Request));
                        lblInstructions.Text = strMessage;
                        lblInstructions.Visible = true;
                        btnCustomizeHomeDir.Visible = false;
                    }

                    txtHomeDirectory.Text = @"Portals/[PortalID]";
                    txtHomeDirectory.Enabled = false;
                    if (MembershipProviderConfig.RequiresQuestionAndAnswer)
                    {
                        questionRow.Visible = true;
                        answerRow.Visible = true;
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        class TemplateDisplayComparer : IComparer<PortalController.PortalTemplateInfo>
        {
            public int Compare(PortalController.PortalTemplateInfo x, PortalController.PortalTemplateInfo y)
            {
                var cultureCompare = String.Compare(x.CultureCode, y.CultureCode, StringComparison.CurrentCulture);
                if (cultureCompare == 0)
                {
                    return String.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
                }

                //put blank cultures last
                if(string.IsNullOrEmpty(x.CultureCode) || string.IsNullOrEmpty(y.CultureCode))
                {
                    cultureCompare *= -1;
                }
                return cultureCompare;
            }
        }

        void BindTemplates()
        {
            var templates = PortalController.Instance.GetAvailablePortalTemplates();
            templates = templates.OrderBy(x => x, new TemplateDisplayComparer()).ToList();

            foreach (var template in templates)
            {
                var item = CreateListItem(template);
                cboTemplate.AddItem(item.Text, item.Value);
            }

            SelectADefaultTemplate(templates);

            if (cboTemplate.Items.Count == 0)
            {
                UI.Skins.Skin.AddModuleMessage(this, "", Localization.GetString("PortalMissing", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                cmdUpdate.Enabled = false;
            }
        }

        void BindSiteGroups()
        {
            var portalGroups = PortalGroupController.Instance.GetPortalGroups().ToList();
            var showGroups = portalGroups.Any();
            if (showGroups)
            {
                cboSiteGroups.Items.Add(new RadComboBoxItem(LocalizeString("None_Specified"), "-1"));
                foreach (var portalGroup in portalGroups)
                {
                    cboSiteGroups.Items.Add(new RadComboBoxItem(portalGroup.PortalGroupName, portalGroup.PortalGroupId.ToString()));
                }
            }
            pnlSiteGroups.Visible = showGroups;
        }

        void SelectADefaultTemplate(IList<PortalController.PortalTemplateInfo> templates)
        {
            string currentCulture = Thread.CurrentThread.CurrentUICulture.Name;

            var defaultTemplates =
                templates.Where(x => Path.GetFileNameWithoutExtension(x.TemplateFilePath) == "Default Website").ToList();

            var match = defaultTemplates.FirstOrDefault(x => x.CultureCode == currentCulture);
            if(match == null)
            {
                match = defaultTemplates.FirstOrDefault(x => x.CultureCode.StartsWith(currentCulture.Substring(0, 2)));
            }
            if(match == null)
            {
                match = defaultTemplates.FirstOrDefault(x => String.IsNullOrEmpty(x.CultureCode));
            }

            if(match != null)
            {
                cboTemplate.SelectedIndex = templates.IndexOf(match);
            }
        }

        ListItem CreateListItem(PortalController.PortalTemplateInfo template)
        {
            string text, value;
            if (string.IsNullOrEmpty(template.CultureCode))
            {
                text = template.Name;
                value = Path.GetFileName(template.TemplateFilePath);
            }
            else
            {
                if (DisplayType == 0)
                {
                    string _ViewType = Convert.ToString(Services.Personalization.Personalization.GetProfile("LanguageDisplayMode", "ViewType" + PortalId));
                    switch (_ViewType)
                    {
                        case "NATIVE":
                            DisplayType = CultureDropDownTypes.NativeName;
                            break;
                        case "ENGLISH":
                            DisplayType = CultureDropDownTypes.EnglishName;
                            break;
                        default:
                            DisplayType = CultureDropDownTypes.DisplayName;
                            break;
                    }
                }

                text = string.Format("{0} - {1}", template.Name, Localization.GetLocaleName(template.CultureCode, DisplayType));
                value = string.Format("{0}|{1}", Path.GetFileName(template.TemplateFilePath), template.CultureCode);
            }
            
            return new ListItem(text, value);
        }
        
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(IsHostMenu ? Globals.NavigateURL() : Globals.GetPortalDomainName(PortalAlias.HTTPAlias, Request, true), true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdUpdate_Click(Object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                PortalController.PortalTemplateInfo template = LoadPortalTemplateInfoForSelectedItem();
                
                try
                {
                    bool blnChild;
                    string strPortalAlias;
                    string strChildPath = string.Empty;
                    var closePopUpStr = string.Empty;

                    //check template validity
                    var messages = new ArrayList();
                    string schemaFilename = Server.MapPath(string.Concat(AppRelativeTemplateSourceDirectory, "portal.template.xsd"));
                    string xmlFilename = template.TemplateFilePath;
                    var xval = new PortalTemplateValidator();
                    if (!xval.Validate(xmlFilename, schemaFilename))
                    {
                        UI.Skins.Skin.AddModuleMessage(this, "", String.Format(Localization.GetString("InvalidTemplate", LocalResourceFile), Path.GetFileName(template.TemplateFilePath)), ModuleMessage.ModuleMessageType.RedError);
                        messages.AddRange(xval.Errors);
                        lstResults.Visible = true;
                        lstResults.DataSource = messages;
                        lstResults.DataBind();
                        validationPanel.Visible = true;
                        return;
                    }

                    //Set Portal Name
					txtPortalAlias.Text = txtPortalAlias.Text.ToLowerInvariant();
					txtPortalAlias.Text = txtPortalAlias.Text.Replace("http://", "");

                    //Validate Portal Name
                    if (!Globals.IsHostTab(PortalSettings.ActiveTab.TabID))
                    {
                        blnChild = true;
						strPortalAlias = txtPortalAlias.Text;
                    }
                    else
                    {
                        blnChild = (optType.SelectedValue == "C");

                        strPortalAlias = blnChild ? PortalController.GetPortalFolder(txtPortalAlias.Text) : txtPortalAlias.Text;
                    }

                    string message = String.Empty;
                    ModuleMessage.ModuleMessageType messageType = ModuleMessage.ModuleMessageType.RedError;
                    if (!PortalAliasController.ValidateAlias(strPortalAlias, blnChild))
                    {
                        message = Localization.GetString("InvalidName", LocalResourceFile);
                    }

                    //check whether have conflict between tab path and portal alias.
                    var checkTabPath = string.Format("//{0}", strPortalAlias);
                    if (TabController.GetTabByTabPath(PortalSettings.PortalId, checkTabPath, string.Empty) != Null.NullInteger
						|| TabController.GetTabByTabPath(Null.NullInteger, checkTabPath, string.Empty) != Null.NullInteger)
                    {
                        message = Localization.GetString("DuplicateWithTab", LocalResourceFile);
                    }

                    //Validate Password
                    if (txtPassword.Text != txtConfirm.Text)
                    {
                        if (!String.IsNullOrEmpty(message)) message += "<br/>";
                        message += Localization.GetString("InvalidPassword", LocalResourceFile);
                    }
                    string strServerPath = Globals.GetAbsoluteServerPath(Request);

                    //Set Portal Alias for Child Portals
                    if (String.IsNullOrEmpty(message))
                    {
                        if (blnChild)
                        {
                            strChildPath = strServerPath + strPortalAlias;

                            if (Directory.Exists(strChildPath))
                            {
                                message = Localization.GetString("ChildExists", LocalResourceFile);
                            }
                            else
                            {
                                if (!Globals.IsHostTab(PortalSettings.ActiveTab.TabID))
                                {
                                    strPortalAlias = Globals.GetDomainName(Request, true) + "/" + strPortalAlias;
                                }
                                else
                                {
									strPortalAlias = txtPortalAlias.Text;
                                }
                            }
                        }
                    }

                    //Get Home Directory
                    string homeDir = txtHomeDirectory.Text != @"Portals/[PortalID]" ? txtHomeDirectory.Text : "";

                    //Validate Home Folder
                    if (!string.IsNullOrEmpty(homeDir))
                    {
                        if (string.IsNullOrEmpty(String.Format("{0}\\{1}\\", Globals.ApplicationMapPath, homeDir).Replace("/", "\\")))
                        {
                            message = Localization.GetString("InvalidHomeFolder", LocalResourceFile);
                        }
                        if (homeDir.Contains("admin") || homeDir.Contains("DesktopModules") || homeDir.ToLowerInvariant() == "portals/")
                        {
                            message = Localization.GetString("InvalidHomeFolder", LocalResourceFile);
                        }
                    }

                    //Validate Portal Alias
                    if (!string.IsNullOrEmpty(strPortalAlias))
                    {
                        PortalAliasInfo portalAlias = PortalAliasController.Instance.GetPortalAlias(strPortalAlias.ToLower());
                        if (portalAlias != null)
                        {
                            message = Localization.GetString("DuplicatePortalAlias", LocalResourceFile);
                        }
                    }

                    //Create Portal
                    if (String.IsNullOrEmpty(message))
                    {
                        //Attempt to create the portal
                        UserInfo adminUser = new UserInfo();
                        int intPortalId;
                        try
                        {
                            if (useCurrent.Checked)
                            {
                                adminUser = UserInfo;
                                intPortalId = PortalController.Instance.CreatePortal(txtPortalName.Text,
                                                                           adminUser.UserID,
                                                                           txtDescription.Text,
                                                                           txtKeyWords.Text,
                                                                           template,
                                                                           homeDir,
                                                                           strPortalAlias,
                                                                           strServerPath,
                                                                           strChildPath,
                                                                           blnChild);
                            }
                            else
                            {
                                adminUser = new UserInfo
                                                {
                                                    FirstName = txtFirstName.Text,
                                                    LastName = txtLastName.Text,
                                                    Username = txtUsername.Text,
                                                    DisplayName = txtFirstName.Text + " " + txtLastName.Text,
                                                    Email = txtEmail.Text,
                                                    IsSuperUser = false,
                                                    Membership =
                                                        {
                                                            Approved = true, 
                                                            Password = txtPassword.Text, 
                                                            PasswordQuestion = txtQuestion.Text, 
                                                            PasswordAnswer = txtAnswer.Text
                                                        },
                                                    Profile =
                                                        {
                                                            FirstName = txtFirstName.Text, 
                                                            LastName = txtLastName.Text
                                                        }
                                                };

                                intPortalId = PortalController.Instance.CreatePortal(txtPortalName.Text,
                                                                           adminUser,
                                                                           txtDescription.Text,
                                                                           txtKeyWords.Text,
                                                                           template,
                                                                           homeDir,
                                                                           strPortalAlias,
                                                                           strServerPath,
                                                                           strChildPath,
                                                                           blnChild);
                            }
							
                        }
                        catch (Exception ex)
                        {
                            intPortalId = Null.NullInteger;
                            message = ex.Message;
                        }

                        if (intPortalId != -1)
                        {
                            //Add new portal to Site Group
                            if (cboSiteGroups.SelectedValue != "-1")
                            {
                                var portal = PortalController.Instance.GetPortal(intPortalId);
                                var portalGroup = PortalGroupController.Instance.GetPortalGroups().SingleOrDefault(g => g.PortalGroupId == Int32.Parse(cboSiteGroups.SelectedValue));
                                if (portalGroup != null)
                                {
                                    PortalGroupController.Instance.AddPortalToGroup(portal, portalGroup, args => { });
                                }
                            }

                            //Create a Portal Settings object for the new Portal
                            PortalInfo objPortal = PortalController.Instance.GetPortal(intPortalId);
                            var newSettings = new PortalSettings { PortalAlias = new PortalAliasInfo { HTTPAlias = strPortalAlias }, PortalId = intPortalId, DefaultLanguage = objPortal.DefaultLanguage };
                            string webUrl = Globals.AddHTTP(strPortalAlias);
                            try
                            {
                                if (!Globals.IsHostTab(PortalSettings.ActiveTab.TabID))
                                {
                                    message = Mail.SendMail(PortalSettings.Email,
                                                               txtEmail.Text,
                                                               PortalSettings.Email + ";" + Host.HostEmail,
                                                               Localization.GetSystemMessage(newSettings, "EMAIL_PORTAL_SIGNUP_SUBJECT", adminUser),
                                                               Localization.GetSystemMessage(newSettings, "EMAIL_PORTAL_SIGNUP_BODY", adminUser),
                                                               "",
                                                               "",
                                                               "",
                                                               "",
                                                               "",
                                                               "");
                                }
                                else
                                {
                                    message = Mail.SendMail(Host.HostEmail,
                                                               txtEmail.Text,
                                                               Host.HostEmail,
                                                               Localization.GetSystemMessage(newSettings, "EMAIL_PORTAL_SIGNUP_SUBJECT", adminUser),
                                                               Localization.GetSystemMessage(newSettings, "EMAIL_PORTAL_SIGNUP_BODY", adminUser),
                                                               "",
                                                               "",
                                                               "",
                                                               "",
                                                               "",
                                                               "");
                                }
                            }
                            catch (Exception exc)
                            {
                                Logger.Error(exc);

                                closePopUpStr = (PortalSettings.EnablePopUps) ? "onclick=\"return " + UrlUtils.ClosePopUp(true,webUrl,true) + "\"" : "";
                                message = string.Format(Localization.GetString("UnknownSendMail.Error", LocalResourceFile), webUrl, closePopUpStr);
                            }
                            EventLogController.Instance.AddLog(PortalController.Instance.GetPortal(intPortalId), PortalSettings, UserId, "", EventLogController.EventLogType.PORTAL_CREATED);

                            // mark default language as published if content localization is enabled
                            bool ContentLocalizationEnabled = PortalController.GetPortalSettingAsBoolean("ContentLocalizationEnabled", PortalId, false);
                            if (ContentLocalizationEnabled)
                            {
                                LocaleController lc = new LocaleController();
                                lc.PublishLanguage(intPortalId, objPortal.DefaultLanguage, true);
                            }

                            //Redirect to this new site
                            if (message == Null.NullString)
                            {
                                webUrl = (PortalSettings.EnablePopUps) ? UrlUtils.ClosePopUp(true, webUrl, false) : webUrl;
                                Response.Redirect(webUrl,true);
                            }
                            else
                            {
                                closePopUpStr = (PortalSettings.EnablePopUps) ? "onclick=\"return " + UrlUtils.ClosePopUp(true, webUrl, true) + "\"" : "";
                                message = string.Format(Localization.GetString("SendMail.Error", LocalResourceFile), message, webUrl, closePopUpStr);
                                messageType = ModuleMessage.ModuleMessageType.YellowWarning;
                            }
                        }
                    }
                    UI.Skins.Skin.AddModuleMessage(this, "", message, messageType);
                }
                catch (Exception exc) //Module failed to load
                {
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
            }
        }

        private void optType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                txtPortalAlias.Text = optType.SelectedValue == "C" ? Globals.GetDomainName(Request) + @"/" : "";
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void btnCustomizeHomeDir_Click(Object sender, EventArgs e)
        {
            try
            {
                if (txtHomeDirectory.Enabled)
                {
                    btnCustomizeHomeDir.Text = Localization.GetString("Customize", LocalResourceFile);
                    txtHomeDirectory.Text = @"Portals/[PortalID]";
                    txtHomeDirectory.Enabled = false;
                }
                else
                {
                    btnCustomizeHomeDir.Text = Localization.GetString("AutoGenerate", LocalResourceFile);
                    txtHomeDirectory.Text = "";
                    txtHomeDirectory.Enabled = true;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cboTemplate_SelectedIndexChanged(Object sender, EventArgs e)
        {
            try
            {
                if (cboTemplate.SelectedIndex > 0)
                {
                    var template = LoadPortalTemplateInfoForSelectedItem();
                    
                    if (!String.IsNullOrEmpty(template.Description))
                    {
                        rowTemplateDescription.Visible = true;
                        lblTemplateDescription.Text = Server.HtmlDecode(template.Description);
                    }
                    else
                    {
                        rowTemplateDescription.Visible = false;
                    }
                }
                else
                {
                    rowTemplateDescription.Visible = false;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        PortalController.PortalTemplateInfo LoadPortalTemplateInfoForSelectedItem()
        {
            var values = cboTemplate.SelectedItem.Value.Split('|');

            return PortalController.Instance.GetPortalTemplate(Path.Combine(TestableGlobals.Instance.HostMapPath, values[0]), values.Length > 1 ? values[1] : null);
        }

        private void useCurrent_CheckedChanged(object sender, EventArgs e)
        {
            adminUserPanel.Visible = !useCurrent.Checked;
        }

    }
}