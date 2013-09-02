#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Globalization;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;

namespace DotNetNuke.Services.Subscriptions.Entities.Templates
{
    public class SubscriptionTypeContent : IPropertyAccess
    {
        #region Constructors

        public SubscriptionTypeContent(SubscriptionType subscriptionType)
        {
            _subscriptionType = subscriptionType;
        }

        #endregion

        #region Private members

        private readonly SubscriptionType _subscriptionType;

        #endregion

        #region Implementation of IPropertyAccess

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "friendlyname":
                    return string.IsNullOrEmpty(_subscriptionType.FriendlyName)
                               ? string.Empty
                               : _subscriptionType.FriendlyName;
                case "subscriptionname":
                    return string.IsNullOrEmpty(_subscriptionType.SubscriptionName)
                               ? string.Empty
                               : _subscriptionType.SubscriptionName;
                default:
                    propertyNotFound = true;
                    return null;
            }
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        #endregion
    }
}