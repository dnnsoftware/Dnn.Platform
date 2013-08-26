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

namespace DotNetNuke.Subscriptions.Components.Entities.Templates
{
    public class ActionsPropertyAccess : IPropertyAccess
    {
        #region Constructors

        public ActionsPropertyAccess(Actions actions)
        {
            Actions = actions;
        }

        #endregion

        #region Private members

        private Actions Actions { get; set; }

        #endregion

        #region Implementation of IPropertyAccess

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "unsubscribeurl":
                    return Actions.UnsubscribeUrl.ToString(formatProvider);
                default:
                    propertyNotFound = true;
                    return null;
            }
        }

        public CacheLevel Cacheability
        {
            get { return CacheLevel.fullyCacheable; }
        }

        #endregion
    }
}