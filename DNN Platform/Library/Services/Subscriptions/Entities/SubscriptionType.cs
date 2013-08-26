#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.Subscriptions.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [Serializable]
    public class SubscriptionType
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public int SubscriptionTypeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string SubscriptionName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string FriendlyName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember()]
        public int DesktopModuleId { get; set; }
    }
}