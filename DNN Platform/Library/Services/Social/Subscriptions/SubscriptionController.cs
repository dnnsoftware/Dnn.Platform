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
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Services.Social.Subscriptions.Data;
using DotNetNuke.Services.Social.Subscriptions.Entities;

namespace DotNetNuke.Services.Social.Subscriptions
{
    public class SubscriptionController : ServiceLocator<ISubscriptionController, SubscriptionController>, ISubscriptionController
    {
        private readonly IDataService dataService;
        private readonly ISubscriptionSecurityController subscriptionSecurityController;

        public SubscriptionController()
        {
            dataService = DataService.Instance;
            subscriptionSecurityController = SubscriptionSecurityController.Instance;
        }

        protected override Func<ISubscriptionController> GetFactory()
        {
            return () => new SubscriptionController();
        }

        #region Implemented Methods
        public IEnumerable<Subscription> GetUserSubscriptions(int userId, int portalId, int subscriptionTypeId = -1)
        {
            var subscriptions = CBO.FillCollection<Subscription>(dataService.GetSubscriptionsByUser(
                userId,
                portalId,
                subscriptionTypeId));

            return subscriptions.Where(s => subscriptionSecurityController.HasPermission(s));
        }

        public IEnumerable<Subscription> GetContentSubscriptions(int portalId, int subscriptionTypeId, string objectKey)
        {
            var subscriptions = CBO.FillCollection<Subscription>(dataService.GetSubscriptionsByContent(
                portalId,
                subscriptionTypeId,
                objectKey));

            return subscriptions.Where(s => subscriptionSecurityController.HasPermission(s));
        }

        public bool IsSubscribed(int userId, int portalId, int subscriptionTypeId, string objectKey, int moduleId = -1, int tabId = -1)
        {
            var subscription = CBO.FillObject<Subscription>(dataService.IsSubscribed(
                userId,
                portalId,
                subscriptionTypeId,
                objectKey,
                moduleId,
                tabId));

            if (subscription == null) return false;

            return subscriptionSecurityController.HasPermission(subscription);
        }

        public int AddSubscription(Subscription subscription)
        {
            Requires.NotNull("subscription", subscription);
            Requires.NotNegative("subscription.UserId", subscription.UserId);
            Requires.NotNegative("subscription.SubscriptionTypeId", subscription.SubscriptionTypeId);
            Requires.NotNull("subscription.ObjectKey", subscription.ObjectKey);

            return dataService.AddSubscription(subscription.UserId,
                                               subscription.PortalId,
                                               subscription.SubscriptionTypeId,
                                               subscription.ObjectKey,
                                               subscription.Description,
                                               subscription.ModuleId,
                                               subscription.TabId);
        }

        public void DeleteSubscription(int subscriptionId)
        {
            Requires.NotNegative("subscriptionId", subscriptionId);

            dataService.DeleteSubscription(subscriptionId);
        }
        #endregion
    }
}
