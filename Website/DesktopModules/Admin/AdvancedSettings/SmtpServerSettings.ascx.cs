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
using System;
using System.Text;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;

namespace DotNetNuke.Modules.Admin.AdvancedSettings
{
    public partial class SmtpServerSettings : PortalModuleBase
    {        
        #region Private Methods

        private void BindData()
        {
            txtSMTPServer.Text = Entities.Host.Host.SMTPServer;
            if (!string.IsNullOrEmpty(Entities.Host.Host.SMTPAuthentication))
            {
                optSMTPAuthentication.Items.FindByValue(Entities.Host.Host.SMTPAuthentication).Selected = true;
            }
            else
            {
                optSMTPAuthentication.Items.FindByValue("0").Selected = true;
            }

            chkSMTPEnableSSL.Checked = Entities.Host.Host.EnableSMTPSSL;
            txtSMTPUsername.Text = Entities.Host.Host.SMTPUsername;
            txtSMTPPassword.Attributes.Add("value", Entities.Host.Host.SMTPPassword);
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            cmdEmail.Click += TestEmail;

            try
            {
                //If this is the first visit to the page, populate the site data
                if (!Page.IsPostBack)
                {
                    BindData();

                    if (Request.QueryString["smtpwarning"] != null)
                    {
                        Skin.AddModuleMessage(this, Localization.GetString("SmtpServerWarning", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                    }
                }

                txtSMTPPassword.Attributes.Add("value", txtSMTPPassword.Text);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void TestEmail(object sender, EventArgs e)
        {
            try
            {
                var hostEmail = Entities.Host.Host.HostEmail;
                if (!String.IsNullOrEmpty(hostEmail))
                {
                    txtSMTPPassword.Attributes.Add("value", txtSMTPPassword.Text);

                    string strMessage = Mail.SendMail(hostEmail,
                                                      hostEmail,
                                                      "",
                                                      "",
                                                      MailPriority.Normal,
                                                      Localization.GetSystemMessage(PortalSettings, "EMAIL_SMTP_TEST_SUBJECT"),
                                                      MailFormat.Text,
                                                      Encoding.UTF8,
                                                      "",
                                                      "",
                                                      txtSMTPServer.Text,
                                                      optSMTPAuthentication.SelectedItem.Value,
                                                      txtSMTPUsername.Text,
                                                      txtSMTPPassword.Text,
                                                      chkSMTPEnableSSL.Checked);
                    if (!String.IsNullOrEmpty(strMessage))
                    {
                        UI.Skins.Skin.AddModuleMessage(this, "", String.Format(Localization.GetString("EmailErrorMessage", LocalResourceFile), strMessage), ModuleMessage.ModuleMessageType.RedError);
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, "", Localization.GetString("EmailSentMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                    }
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, "", Localization.GetString("SpecifyHostEmailMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        #region Public Methods

        public void Update(ref string redirectUrl)
        {
            var smtpServer = txtSMTPServer.Text;
            var smtpWarning = !string.IsNullOrEmpty(smtpServer)
                    && smtpServer != Entities.Host.Host.SMTPServer
                    && smtpServer.Contains(":")
                    && smtpServer.Split(':')[1] != "25"
                    && !SecurityPolicy.HasAspNetHostingPermission();

            HostController.Instance.Update("SMTPServer", txtSMTPServer.Text, false);
            HostController.Instance.Update("SMTPAuthentication", optSMTPAuthentication.SelectedItem.Value, false);
            HostController.Instance.Update("SMTPUsername", txtSMTPUsername.Text, false);
            HostController.Instance.UpdateEncryptedString("SMTPPassword", txtSMTPPassword.Text, Config.GetDecryptionkey());
            HostController.Instance.Update("SMTPEnableSSL", chkSMTPEnableSSL.Checked ? "Y" : "N", false);

            if (smtpWarning && redirectUrl.IndexOf("smtpwarning=true", StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                redirectUrl = string.Format("{0}{1}smtpwarning=true", redirectUrl, redirectUrl.Contains("?") ? "&" : "?");
            }
        }

        #endregion
    }
}