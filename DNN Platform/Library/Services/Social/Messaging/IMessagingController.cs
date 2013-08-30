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

using System;
using System.Collections.Generic;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Social.Notifications;

namespace DotNetNuke.Services.Social.Messaging
{
    public interface IMessagingController
    {
        #region Public APIs

        void SendMessage(Message message, IList<RoleInfo> roles, IList<UserInfo> users, IList<int> fileIDs);

        void SendMessage(Message message, IList<RoleInfo> roles, IList<UserInfo> users, IList<int> fileIDs, UserInfo sender);

        #endregion        

		#region Toast APIS

		/// <summary>
		/// Is there a Toast that needs to be sent for a Notification
		/// </summary>
		/// <param name="notificationId">The Notification Id </param>
		/// <returns>True if Toast needs to be sent. False otherwise.</returns>
		bool IsToastPending(int notificationId);

		/// <summary>
		/// Mark a Toast ready for sending
		/// </summary>
		/// <param name="notification">The notification Object </param>
		/// <param name="userInfo">The Recipient User Info Object</param>
		void MarkReadyForToast(Notification notification, UserInfo userInfo);

		/// <summary>
		/// Mark Toast being already sent
		/// </summary>
		/// <param name="notificationId">The notification Id </param>
		/// <param name="userId">The Recipient User Id </param>
		void MarkToastSent(int notificationId, int userId);

		/// <summary>
		/// Get a list of Toasts that have not been delivered yet.
		/// </summary>
		/// <param name="userInfo">UserInfo object</param>
		/// <returns>List of Undelivered Toasts for the user specific to the portal</returns>
		IList<Message> GetToasts(UserInfo userInfo);

		#endregion
    }
}