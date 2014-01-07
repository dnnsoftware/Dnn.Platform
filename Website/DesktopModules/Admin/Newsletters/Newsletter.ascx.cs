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
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Newsletters
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Newsletter PortalModuleBase is used to manage a Newsletters
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	2004-09-13	Updated to reflect design changes for Help, 508 support and localisation
    ///     [lpointer]  2006-02-03  Added 'From' email address support.
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class Newsletter : PortalModuleBase
    {

        #region Private Methods

        /// <summary>
        /// helper function for ManageDirectoryBase
        /// <history>
        /// [vmasanas] 2007-09-07 added
        /// [sleupold] 2008-02-10 support for local links added
        /// </history>
        /// </summary>
        private static string FormatUrls(Match m)
        {
            var match = m.Value;
            var url = m.Groups["url"].Value;
            var result = match;
            if (url.StartsWith("/")) //server absolute path
            {
                return result.Replace(url, Globals.AddHTTP(HttpContext.Current.Request.Url.Host) + url);
            }
            return url.Contains("://") || url.Contains("mailto:") ? result : result.Replace(url, Globals.AddHTTP(HttpContext.Current.Request.Url.Host) + Globals.ApplicationPath + "/" + url);
        }

        #endregion

        #region Protected Methods

        protected string GetInitialEntries()
            {
            int id;
            UserInfo user;
            RoleInfo role;
            var entities = new StringBuilder("[");
            var roleController = new RoleController();

            foreach (var value in (Request.QueryString["users"] ?? string.Empty).Split(','))
                if (int.TryParse(value, out id) && (user = UserController.GetUserById(PortalId, id)) != null)
                    entities.AppendFormat(@"{{ id: ""user-{0}"", name: ""{1}"" }},", user.UserID, user.DisplayName.Replace("\"", ""));
            foreach (var value in (Request.QueryString["roles"] ?? string.Empty).Split(','))
                if (int.TryParse(value, out id) && (role = roleController.GetRole(id, PortalId)) != null)
                    entities.AppendFormat(@"{{ id: ""role-{0}"", name: ""{1}"" }},", role.RoleID, role.RoleName.Replace("\"", ""));

            return entities.Append(']').ToString();
            }
        #endregion

        #region Event Handlers

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <history>
        /// 	[cnurse]	2004-09-10	Updated to reflect design changes for Help, 508 support and localisation
        ///     [sleupold]  2007-10-07  Relay address and Language adoption added
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdPreview.Click += OnPreviewClick;
            cmdSend.Click += OnSendClick;

            ServicesFramework.Instance.RequestAjaxScriptSupport();
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            jQuery.RequestDnnPluginsRegistration();

            try
            {
                if (!Page.IsPostBack)
                {
                    txtFrom.Text = UserInfo.Email;
                }
                plLanguages.Visible = (LocaleController.Instance.GetLocales(PortalId).Count > 1);
                pnlRelayAddress.Visible = (cboSendMethod.SelectedValue == "RELAY");
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdSend_Click runs when the cmdSend Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	2004-09-10	Updated to reflect design changes for Help, 508 support and localisation
        ///     [sleupold]	2007-08-15	added support for tokens and SendTokenizedBulkEmail
        ///     [sleupold]  2007-09-09   moved Roles to SendTokenizedBulkEmail
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnSendClick(Object sender, EventArgs e)
        {
            var message = "";
            var messageType = ModuleMessage.ModuleMessageType.GreenSuccess;

            try
            {
                List<string> roleNames;
                List<UserInfo> users;
                GetRecipients(out roleNames, out users);

                if(IsReadyToSend(roleNames, users, ref message, ref messageType))
                {
                    SendEmail(roleNames, users, ref message, ref messageType);
                }
                
                UI.Skins.Skin.AddModuleMessage(this, message, messageType);
                ((CDefault) Page).ScrollToControl(Page.Form);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void SendEmail(List<string> roleNames, List<UserInfo> users, ref string message, ref ModuleMessage.ModuleMessageType messageType)
        {
            //it is awkward to ensure that email is disposed correctly because when sent asynch it should be disposed by the  asynch thread
			var email = new SendTokenizedBulkEmail(roleNames, users, /*removeDuplicates*/ true, txtSubject.Text, ConvertToAbsoluteUrls(teMessage.Text));

            bool isValid;
            try
            {
                isValid = PrepareEmail(email, ref message, ref messageType);
            }
            catch(Exception)
            {
                email.Dispose();
                throw;
            }

            if (isValid)
            {

                if (optSendAction.SelectedItem.Value == "S")
                {
                    try
                    {
                        SendMailSynchronously(email, out message, out messageType);
                    }
                    finally
                    {
                        email.Dispose();
                    }
                }
                else
                {
                    SendMailAsyncronously(email, out message, out messageType);
                    //dispose will be handled by async thread
                }
            }
        }

        private void SendMailAsyncronously(SendTokenizedBulkEmail email, out string message, out ModuleMessage.ModuleMessageType messageType)
        {
            //First send off a test message
            var strStartSubj = Localization.GetString("EMAIL_BulkMailStart_Subject.Text", Localization.GlobalResourceFile);
            if (!string.IsNullOrEmpty(strStartSubj)) strStartSubj = string.Format(strStartSubj, txtSubject.Text);
            
            var strStartBody = Localization.GetString("EMAIL_BulkMailStart_Body.Text", Localization.GlobalResourceFile);
            if (!string.IsNullOrEmpty(strStartBody)) strStartBody = string.Format(strStartBody, txtSubject.Text, UserInfo.DisplayName, email.Recipients().Count);

            var sendMailResult = Mail.SendMail(txtFrom.Text,
                txtFrom.Text,
                "",
                "",
                MailPriority.Normal,
                strStartSubj,
                MailFormat.Text,
                Encoding.UTF8,
                strStartBody,
                "",
                Host.SMTPServer,
                Host.SMTPAuthentication,
                Host.SMTPUsername,
                Host.SMTPPassword,
                Host.EnableSMTPSSL);

            if (string.IsNullOrEmpty(sendMailResult))
            {
                var objThread = new Thread(() => SendAndDispose(email));
                objThread.Start();
                message = Localization.GetString("MessageSent", LocalResourceFile);
                messageType = ModuleMessage.ModuleMessageType.GreenSuccess;
            }
            else
            {
                message = string.Format(Localization.GetString("NoMessagesSentPlusError", LocalResourceFile), sendMailResult);
                messageType = ModuleMessage.ModuleMessageType.YellowWarning;
            }
        }

        private void SendAndDispose(SendTokenizedBulkEmail email)
        {
            using(email)
            {
                email.Send();
            }
        }

        private void SendMailSynchronously(SendTokenizedBulkEmail email, out string strResult, out ModuleMessage.ModuleMessageType msgResult)
        {
            int mailsSent = email.SendMails();

            if (mailsSent > 0)
            {
                strResult = string.Format(Localization.GetString("MessagesSentCount", LocalResourceFile), mailsSent);
                msgResult = ModuleMessage.ModuleMessageType.GreenSuccess;
            }
            else
            {
                strResult = string.Format(Localization.GetString("NoMessagesSent", LocalResourceFile), email.SendingUser.Email);
                msgResult = ModuleMessage.ModuleMessageType.YellowWarning;
            }
        }

        private bool PrepareEmail(SendTokenizedBulkEmail email, ref string message, ref ModuleMessage.ModuleMessageType messageType)
        {
            bool isValid = true;

            switch (teMessage.Mode)
            {
                case "RICH":
                    email.BodyFormat = MailFormat.Html;
                    break;
                default:
                    email.BodyFormat = MailFormat.Text;
                    break;
            }
                        
            switch (cboPriority.SelectedItem.Value)
            {
                case "1":
                    email.Priority = MailPriority.High;
                    break;
                case "2":
                    email.Priority = MailPriority.Normal;
                    break;
                case "3":
                    email.Priority = MailPriority.Low;
                    break;
                default:
                    isValid = false;
                    break;
            }

            if (txtFrom.Text != string.Empty && email.SendingUser.Email != txtFrom.Text)
            {
                var myUser = email.SendingUser ?? new UserInfo();
                myUser.Email = txtFrom.Text;
                email.SendingUser = myUser;
            }

            if (txtReplyTo.Text != string.Empty && email.ReplyTo.Email != txtReplyTo.Text)
            {
                var myUser = new UserInfo {Email = txtReplyTo.Text};
                email.ReplyTo = myUser;
            }

            if (selLanguage.Visible && selLanguage.SelectedLanguages != null)
            {
                email.LanguageFilter = selLanguage.SelectedLanguages;
            }

            if (ctlAttachment.Url.StartsWith("FileID="))
            {
                int fileId = int.Parse(ctlAttachment.Url.Substring(7));
                var objFileInfo = FileManager.Instance.GetFile(fileId);
                //TODO: support secure storage locations for attachments! [sleupold 06/15/2007]
                email.AddAttachment(FileManager.Instance.GetFileContent(objFileInfo), 
                                               new ContentType { MediaType = objFileInfo.ContentType, Name = objFileInfo.FileName });
            }

            switch (cboSendMethod.SelectedItem.Value)
            {
                case "TO":
                    email.AddressMethod = SendTokenizedBulkEmail.AddressMethods.Send_TO;
                    break;
                case "BCC":
                    email.AddressMethod = SendTokenizedBulkEmail.AddressMethods.Send_BCC;
                    break;
                case "RELAY":
                    email.AddressMethod = SendTokenizedBulkEmail.AddressMethods.Send_Relay;
                    if (string.IsNullOrEmpty(txtRelayAddress.Text))
                    {
                        message = string.Format(Localization.GetString("MessagesSentCount", LocalResourceFile), -1);
                        messageType = ModuleMessage.ModuleMessageType.YellowWarning;
                        isValid = false;
                    }
                    else
                    {
                        email.RelayEmailAddress = txtRelayAddress.Text;
                    }
                    break;
            }

            email.SuppressTokenReplace = !chkReplaceTokens.Checked;
            
            return isValid;
        }

        private bool IsReadyToSend(List<string> roleNames, List<UserInfo> users, ref string message, ref ModuleMessage.ModuleMessageType messageType)
        {
             if (String.IsNullOrEmpty(txtSubject.Text) || String.IsNullOrEmpty(teMessage.Text))
            {
                message = Localization.GetString("MessageValidation", LocalResourceFile);
                messageType = ModuleMessage.ModuleMessageType.RedError;
                return false;
            }
                 
            if (users.Count == 0 && roleNames.Count == 0)
            {
                message = string.Format(Localization.GetString("NoRecipients", LocalResourceFile), -1);
                messageType = ModuleMessage.ModuleMessageType.YellowWarning;
                return false;
            }

            return true;
        }

        private void GetRecipients(out List<string> objRoleNames, out List<UserInfo> objUsers)
        {
            objRoleNames = new List<string>();
            objUsers = new List<UserInfo>();

            if (!String.IsNullOrEmpty(txtEmail.Text))
            {
                Array arrEmail = txtEmail.Text.Split(';');
                foreach (string strEmail in arrEmail)
                {
                    var objUser = new UserInfo {UserID = Null.NullInteger, Email = strEmail, DisplayName = strEmail};
                    objUsers.Add(objUser);
                }
            }

            var roleController = new RoleController();
            objRoleNames.AddRange(recipients.Value.Split(',').Where(value => value.StartsWith("role-")).Select(value => roleController.GetRole(int.Parse(value.Substring(5)), PortalId).RoleName).Where(roleName => !string.IsNullOrWhiteSpace(roleName)));
            objUsers.AddRange(recipients.Value.Split(',').Where(value => value.StartsWith("user-")).Select(value => UserController.GetUserById(PortalId, int.Parse(value.Substring(5)))).Where(user => user != null));
        }

        /// <summary>
        /// Display a preview of the message to be sent out
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">ignores</param>
        /// <history>
        /// 	[vmasanas]	2007-09-07  added
        /// </history>
        protected void OnPreviewClick(object sender, EventArgs e)
        {
            try
            {
                if (String.IsNullOrEmpty(txtSubject.Text) || String.IsNullOrEmpty(teMessage.Text))
                {
					//no subject or message
                    var strResult = Localization.GetString("MessageValidation", LocalResourceFile);
                    const ModuleMessage.ModuleMessageType msgResult = ModuleMessage.ModuleMessageType.YellowWarning;
                    UI.Skins.Skin.AddModuleMessage(this, strResult, msgResult);
                    ((CDefault) Page).ScrollToControl(Page.Form);
                    return;
                }
				
				//convert links to absolute
				var strBody = ConvertToAbsoluteUrls(teMessage.Text);

                if (chkReplaceTokens.Checked)
                {
                    var objTr = new TokenReplace();
                    if (cboSendMethod.SelectedItem.Value == "TO")
                    {
                        objTr.User = UserInfo;
                        objTr.AccessingUser = UserInfo;
                        objTr.DebugMessages = true;
                    }
                    lblPreviewSubject.Text = objTr.ReplaceEnvironmentTokens(txtSubject.Text);
                    lblPreviewBody.Text = objTr.ReplaceEnvironmentTokens(strBody);
                }
                else
                {
                    lblPreviewSubject.Text = txtSubject.Text;
                    lblPreviewBody.Text = strBody;
                }
                pnlPreview.Visible = true;
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

		private static string ConvertToAbsoluteUrls(string content)
		{
			//convert links to absolute
			const string pattern = "<(a|link|img|script|object).[^>]*(href|src|action)=(\\\"|'|)(?<url>(.[^\\\"']*))(\\\"|'|)[^>]*>";
			return Regex.Replace(content, pattern, FormatUrls);
		}

        #endregion

    }
}