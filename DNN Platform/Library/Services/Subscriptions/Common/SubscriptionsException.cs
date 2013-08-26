#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;

namespace DotNetNuke.Services.Subscriptions.Common
{
    public class SubscriptionsException : ApplicationException
    {
        public SubscriptionsException()
        {}

        public SubscriptionsException(string msg)
            : base(msg)
        {}

        public SubscriptionsException(string msg, Exception innerException)
            : base(msg, innerException)
        {}
    }
}