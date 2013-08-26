#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Runtime.Serialization;

namespace DotNetNuke.Subscriptions.Components.Entities
{
    [DataContract]
    public class Actions
    {
        #region Public properties

        /// <summary>
        /// A URL the user can visit to alter his Subscription options, or unsubscribe.
        /// </summary>
        [DataMember(Name = "unsubscribeUrl")]
        public string UnsubscribeUrl { get; set; }

        #endregion
    }
}