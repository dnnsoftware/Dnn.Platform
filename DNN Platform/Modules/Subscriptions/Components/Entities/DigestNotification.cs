#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotNetNuke.Subscriptions.Components.Entities
{
    [DataContract]
    public class DigestNotification
    {
        #region Public members

        /// <summary>
        /// The Subscription associated with this notification
        /// </summary>
        [IgnoreDataMember]
        public Subscriber Subscriber { get; set; }

        #endregion
    }
}