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
using System.Data;
using DotNetNuke.Data;
using DotNetNuke.Framework;

namespace DotNetNuke.Services.Social.Subscriptions.Data
{
    public class DataService : ServiceLocator<IDataService, DataService>, IDataService
    {
        #region Private Members
        private readonly DataProvider provider;
        #endregion

        public DataService()
        {
            provider = DataProvider.Instance();
        }

        protected override Func<IDataService> GetFactory()
        {
            return () => new DataService();
        }

        #region Subscription Types
        public int AddSubscriptionType(string subscriptionName, string friendlyName, int desktopModuleId)
        {
            return provider.ExecuteScalar<int>("CoreMessaging_AddSubscriptionType", subscriptionName, friendlyName, desktopModuleId);
        }

        public IDataReader GetSubscriptionTypes()
        {
            return provider.ExecuteReader("CoreMessaging_GetSubscriptionTypes");
        }

        public bool DeleteSubscriptionType(int subscriptionTypeId)
        {
            return provider.ExecuteScalar<int>("CoreMessaging_DeleteSubscriptionType", subscriptionTypeId) == 0;
        }
        #endregion


        #region Subscriptions
        public int AddSubscription(int userId, int portalId, int subscriptionTypeId, string objectKey, string description, int moduleId, int tabId, string objectData)
        {
            return provider.ExecuteScalar<int>("CoreMessaging_AddSubscription", 
                userId, 
                provider.GetNull(portalId),
                subscriptionTypeId, 
                objectKey, 
                description,
                provider.GetNull(moduleId),
                provider.GetNull(tabId),
                objectData);
        }

        public IDataReader GetSubscriptionsByUser(int portalId, int userId, int subscriptionTypeId)
        {
            return provider.ExecuteReader("CoreMessaging_GetSubscriptionsByUser", provider.GetNull(portalId), userId, provider.GetNull(subscriptionTypeId));
        }

        public IDataReader GetSubscriptionsByContent(int portalId, int subscriptionTypeId, string objectKey)
        {
            return provider.ExecuteReader("CoreMessaging_GetSubscriptionsByContent", provider.GetNull(portalId), subscriptionTypeId, objectKey);
        }

        public IDataReader IsSubscribed(int portalId, int userId, int subscriptionTypeId, string objectKey, int moduleId, int tabId)
        {
            return provider.ExecuteReader("CoreMessaging_IsSubscribed",
                provider.GetNull(portalId), 
                userId, 
                subscriptionTypeId, 
                objectKey, 
                provider.GetNull(moduleId),
                provider.GetNull(tabId));
        }

        public bool DeleteSubscription(int subscriptionId)
        {
            return provider.ExecuteScalar<int>("CoreMessaging_DeleteSubscription", subscriptionId) == 0;
        }
        #endregion
    }
}
