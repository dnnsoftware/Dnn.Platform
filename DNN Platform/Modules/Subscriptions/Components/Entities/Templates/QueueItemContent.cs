#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Globalization;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;

namespace DotNetNuke.Subscriptions.Components.Entities
{
    public class QueueItemContent : IPropertyAccess
    {
        #region Constructors

        public QueueItemContent(QueueItem queueItem)
        {
            _queueItem = queueItem;
        }

        #endregion

        #region Private members

        private readonly QueueItem _queueItem;

        #endregion

        #region Implementation of IPropertyAccess

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "subject":
                    return string.IsNullOrEmpty(_queueItem.Subject)
                        ? string.Empty
                        : _queueItem.Subject.ToString(formatProvider);
                case "body":
                    return string.IsNullOrEmpty(_queueItem.Body)
                        ? string.Empty
                        : _queueItem.Body.ToString(formatProvider);
                case "summary":
                    return string.IsNullOrEmpty(_queueItem.Summary)
                        ? string.Empty
                        : _queueItem.Summary.ToString(formatProvider);
                case "objectkey":
                    return string.IsNullOrEmpty(_queueItem.ObjectKey)
                        ? string.Empty
                        : _queueItem.ObjectKey.ToString(formatProvider);
                case "createdondate":
                    return _queueItem.CreatedOnDate.ToString(format, formatProvider);
                case "contenttitle":
                    return string.IsNullOrEmpty(_queueItem.ContentTitle)
                        ? string.Empty
                        :_queueItem.ContentTitle.ToString(formatProvider);
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