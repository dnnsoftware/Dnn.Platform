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
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Social.Messaging.Data;

namespace DotNetNuke.Services.Social.Messaging
{
    public class UserPreferencesController : ServiceLocator<IUserPreferencesController, UserPreferencesController>, IUserPreferencesController
    {
        #region Private Memebers
        private readonly IDataService dataService;
        #endregion

        protected override Func<IUserPreferencesController> GetFactory()
        {
            return () => new UserPreferencesController();
        }

        #region Constructor
        public UserPreferencesController() : this(DataService.Instance)
        {
        }

        public UserPreferencesController(IDataService dataService)
        {
            //Argument Contract
            Requires.NotNull("dataService", dataService);

            this.dataService = dataService;
        }
        #endregion

        #region Public API
        public void SetUserPreference(UserPreference userPreference)
        {
            dataService.SetUserPreference(userPreference.PortalId, userPreference.UserId, Convert.ToInt32(userPreference.MessagesEmailFrequency), Convert.ToInt32(userPreference.NotificationsEmailFrequency));
        }

        public UserPreference GetUserPreference(UserInfo userinfo)
        {
            return CBO.FillObject<UserPreference>(dataService.GetUserPreference(userinfo.PortalID, userinfo.UserID));
        }

        #endregion

    }
}
