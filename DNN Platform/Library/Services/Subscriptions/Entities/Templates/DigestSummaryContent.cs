#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Globalization;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;

namespace DotNetNuke.Subscriptions.Components.Entities.Templates
{
    public class DigestSummaryContent : IPropertyAccess
    {
        #region Constructors

        public DigestSummaryContent(DigestNotification digest)
        {
            _digest = digest;
        }

        #endregion

        #region Private members

        private readonly DigestNotification _digest;

        #endregion

        #region Implementation of IPropertyAccess

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "itemcount":
                    return _digest.QueueItems != null
                        ? _digest.QueueItems.Count.ToString(format, formatProvider)
                        : 0.ToString(format, formatProvider);
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