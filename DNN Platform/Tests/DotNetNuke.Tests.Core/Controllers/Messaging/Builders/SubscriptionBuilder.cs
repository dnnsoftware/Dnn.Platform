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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Social.Subscriptions.Entities;
using DotNetNuke.Tests.Utilities;

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Builders
{
    public class SubscriptionBuilder
    {
        private int subscriptionId;
        private int userId;
        private int portalId;
        private int subscriptionTypeId;
        private string objectKey;
        private string description;
        private int moduleId;
        private int tabId;
        private string objectData;

        internal SubscriptionBuilder()
        {
            subscriptionId = 1;
            userId = Constants.USER_InValidId;
            subscriptionTypeId = 1;
            portalId = Constants.PORTAL_ValidPortalId;
            moduleId = Null.NullInteger;
            tabId = Null.NullInteger;
            objectKey = "content";
            description = "my content description";
            objectData = "";
        }

        internal SubscriptionBuilder WithSubscriptionId(int subscriptionId)
        {
            this.subscriptionId = subscriptionId;
            return this;
        }

        internal SubscriptionBuilder WithUserId(int userId)
        {
            this.userId = userId;
            return this;
        }

        internal SubscriptionBuilder WithPortalId(int portalId)
        {
            this.portalId = portalId;
            return this;
        }

        internal SubscriptionBuilder WithSubscriptionTypeId(int subscriptionTypeId)
        {
            this.subscriptionTypeId = subscriptionTypeId;
            return this;
        }

        internal SubscriptionBuilder WithObjectKey(string objectKey)
        {
            this.objectKey = objectKey;
            return this;
        }

        internal SubscriptionBuilder WithDescription(string description)
        {
            this.description = description;
            return this;
        }
        
        internal SubscriptionBuilder WithModuleId(int moduleId)
        {
            this.moduleId = moduleId;
            return this;
        }

        internal SubscriptionBuilder WithTabId(int tabId)
        {
            this.tabId = tabId;
            return this;
        } 

        internal Subscription Build()
        {
            return new Subscription
                       {
                           SubscriptionTypeId = subscriptionTypeId,
                           SubscriptionId = subscriptionId,
                           CreatedOnDate = DateTime.UtcNow,
                           ModuleId = moduleId,
                           ObjectKey = objectKey,
                           Description = description,
                           PortalId = portalId,
                           TabId = tabId,
                           UserId = userId,
                           ObjectData = objectData
                       };

        }
    }
}
