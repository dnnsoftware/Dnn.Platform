#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Messaging.Data;
using DotNetNuke.Services.Social.Messaging;
using DotNetNuke.Services.Social.Messaging.Internal;

using Message = DotNetNuke.Services.Messaging.Data.Message;

#endregion

namespace DotNetNuke.Services.Messaging
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The Controller class for Messaging
    /// </summary>
    /// <remarks>
    /// </remarks>
    [Obsolete("Deprecated in DNN 6.2.0, please use DotNetNuke.Services.Social.Messaging.MessagingController")]
    public class MessagingController : IMessagingController
    {
        private static TabInfo _MessagingPage;

        #region "Constructors"

        public MessagingController()
            : this(GetDataService())
        {
        }

        public MessagingController(IMessagingDataService dataService)
        {
            _DataService = dataService;
        }

        #endregion

        #region "Private Shared Methods"

        private static IMessagingDataService GetDataService()
        {
            var ds = ComponentFactory.GetComponent<IMessagingDataService>();

            if (ds == null)
            {
                ds = new MessagingDataService();
                ComponentFactory.RegisterComponentInstance<IMessagingDataService>(ds);
            }
            return ds;
        }

        #endregion

        #region "Obsolete Methods"

        public static string DefaultMessagingURL(string ModuleFriendlyName)
        {
            TabInfo page = MessagingPage(ModuleFriendlyName);
            if (page != null)
            {
                return MessagingPage(ModuleFriendlyName).FullUrl;
            }
            else
            {
                return null;
            }
        }

        public static TabInfo MessagingPage(string ModuleFriendlyName)
        {
            if (((_MessagingPage != null)))
            {
                return _MessagingPage;
            }

            ModuleInfo md = ModuleController.Instance.GetModuleByDefinition(PortalSettings.Current.PortalId, ModuleFriendlyName);
            if ((md != null))
            {
                var a = ModuleController.Instance.GetTabModulesByModule(md.ModuleID);
                if ((a != null))
                {
                    var mi = a[0];
                    if ((mi != null))
                    {
                        _MessagingPage = TabController.Instance.GetTab(mi.TabID, PortalSettings.Current.PortalId, false);
                    }
                }
            }

            return _MessagingPage;
        }

        public Message GetMessageByID(int PortalID, int UserID, int messageId)
        {
            var coreMessage = InternalMessagingController.Instance.GetMessage(messageId);
            var coreMessageRecipient = InternalMessagingController.Instance.GetMessageRecipient(messageId, UserID);
            return ConvertCoreMessageToServicesMessage(PortalID, UserID, coreMessageRecipient, coreMessage);
        }

        public List<Message> GetUserInbox(int PortalID, int UserID, int PageNumber, int PageSize)
        {
            return CBO.FillCollection<Message>(_DataService.GetUserInbox(PortalID, UserID, PageNumber, PageSize));
        }

        public int GetInboxCount(int PortalID, int UserID)
        {
            return InternalMessagingController.Instance.CountConversations(UserID, PortalID);
        }

        public int GetNewMessageCount(int PortalID, int UserID)
        {
            return InternalMessagingController.Instance.CountUnreadMessages(UserID, PortalID);
        }

        public Message GetNextMessageForDispatch(Guid SchedulerInstance)
        {
            //does not need to run as scheduled task name was updated 
            return null;
        }

        public void SaveMessage(Message message)
        {
            if ((PortalSettings.Current != null))
            {
                message.PortalID = PortalSettings.Current.PortalId;
            }

            if ((message.Conversation == null || message.Conversation == Guid.Empty))
            {
                message.Conversation = Guid.NewGuid();
            }

            var users = new List<UserInfo>();

            users.Add(UserController.Instance.GetUser(message.PortalID, message.ToUserID));

            List<RoleInfo> emptyRoles = null;
            List<int> files = null;
            
            var coremessage = new Social.Messaging.Message {Body = message.Body, Subject = message.Subject};


            Social.Messaging.MessagingController.Instance.SendMessage(coremessage, emptyRoles, users, files);
        }

        public void UpdateMessage(Message message)
        {
            var user = UserController.Instance.GetCurrentUserInfo().UserID;
            switch (message.Status)
            {
                case MessageStatusType.Unread:
                    InternalMessagingController.Instance.MarkUnRead(message.MessageID, user);
                    break;
                case MessageStatusType.Draft:
                    //no equivalent
                    break;
                case MessageStatusType.Deleted:
                    InternalMessagingController.Instance.MarkArchived(message.MessageID, user);
                    break;
                case MessageStatusType.Read:
                    InternalMessagingController.Instance.MarkRead(message.MessageID, user);
                    break;
            }

            _DataService.UpdateMessage(message);
        }

        public void MarkMessageAsDispatched(int MessageID)
        {
            //does not need to run as scheduled task name was updated
        }

        #endregion

        #region "functions to support obsolence"
        private static Message ConvertCoreMessageToServicesMessage(int PortalID, int UserID, MessageRecipient coreMessageRecipeint, Social.Messaging.Message coreMessage)
        {
            var message = new Message { AllowReply = true, Body = coreMessage.Body, FromUserID = coreMessage.SenderUserID, MessageDate = coreMessage.CreatedOnDate, PortalID = PortalID };

            switch (coreMessageRecipeint.Read)
            {
                case true:
                    message.Status = MessageStatusType.Read;
                    break;
                case false:
                    message.Status = MessageStatusType.Unread;
                    break;
            }

            message.ToUserID = UserID;
            return message;
        }
        #endregion

        private readonly IMessagingDataService _DataService;
    }
}
