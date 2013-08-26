#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Collections.Generic;
using System.Runtime.Serialization;
using DotNetNuke.Entities.Users;


namespace DotNetNuke.Subscriptions.Components.Entities
{
    /// <summary>
    /// Represents a final, formatted, user-ready notification sourced from a <seealso cref="InstantNotification" /> or a
    /// <seealso cref="DigestNotification" /> object and run through the templating mechanism, <see cref="ITemplateController" />.
    /// </summary>
    [DataContract]
    public class FormattedNotification
    {
        #region Constructors

        public FormattedNotification()
        {
            Subscribers = new List<Subscriber>();
        }

        public FormattedNotification(UserInfo author, IList<Subscriber> subscribers)
        {
            Author = author;

            Subscribers = subscribers;
        }

        #endregion

        #region Public members

        [DataMember]
        public UserInfo Author { get; set; }

        [DataMember]
        public IList<Subscriber> Subscribers { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string Body { get; set; }

        #endregion
    }
}