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

namespace DotNetNuke.Subscriptions.Components.Entities
{
    public class QueueItemPropertyAccess : IPropertyAccess
    {
        #region Constructors

        public QueueItemPropertyAccess(QueueItem queueItem)
        {
            QueueItem = queueItem;
        }

        #endregion

        #region Private members

        private QueueItem QueueItem { get; set; }

        #endregion

        #region Implementation of IPropertyAccess

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            throw new NotImplementedException();
        }

        public CacheLevel Cacheability
        {
            get { return CacheLevel.fullyCacheable; }
        }

        #endregion         
    }
}