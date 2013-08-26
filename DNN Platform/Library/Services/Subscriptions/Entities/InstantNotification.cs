#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Collections.Generic;

namespace DotNetNuke.Services.Subscriptions.Entities
{
    public class InstantNotification
    {
        #region Public members

        /// <summary>
        /// The Subscription associated with this notification
        /// </summary>
        public IList<Subscriber> Subscribers { get; set; }


        #endregion
    }
}