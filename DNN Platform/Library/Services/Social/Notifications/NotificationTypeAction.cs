// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Notifications
{
    using System;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Services.Social.Notifications
    /// Class:      NotificationTypeAction
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The NotificationTypeAction class describes a single notification type action that can be associated to a message.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class NotificationTypeAction : BaseEntityInfo, IHydratable
    {
        private int _notificationTypeActionId = -1;

        /// <summary>
        /// Gets or sets the notification type action identifier.
        /// </summary>
        [XmlAttribute]
        public int NotificationTypeActionId
        {
            get
            {
                return this._notificationTypeActionId;
            }

            set
            {
                this._notificationTypeActionId = value;
            }
        }

        /// <summary>
        /// Gets or sets the notification type identifier.
        /// </summary>
        [XmlAttribute]
        public int NotificationTypeId { get; set; }

        /// <summary>
        /// Gets or sets the notification type action name resource key.
        /// </summary>
        [XmlAttribute]
        public string NameResourceKey { get; set; }

        /// <summary>
        /// Gets or sets the notification type action description resource key.
        /// </summary>
        [XmlAttribute]
        public string DescriptionResourceKey { get; set; }

        /// <summary>
        /// Gets or sets the notification type action confirmation resource key.
        /// </summary>
        [XmlAttribute]
        public string ConfirmResourceKey { get; set; }

        /// <summary>
        /// Gets or sets the notification type action order to be used while displaying the list of available actions.
        /// </summary>
        [XmlAttribute]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the Service Framework URL to be called when the action is performed.
        /// </summary>
        [XmlAttribute]
        public string APICall { get; set; }

        /// <summary>
        /// Gets or sets iHydratable.KeyID.
        /// </summary>
        [XmlIgnore]
        public int KeyID
        {
            get { return this.NotificationTypeActionId; }
            set { this.NotificationTypeActionId = value; }
        }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            this.NotificationTypeActionId = Convert.ToInt32(dr["NotificationTypeActionID"]);
            this.NotificationTypeId = Convert.ToInt32(dr["NotificationTypeID"]);
            this.NameResourceKey = dr["NameResourceKey"].ToString();
            this.DescriptionResourceKey = Null.SetNullString(dr["DescriptionResourceKey"]);
            this.ConfirmResourceKey = Null.SetNullString(dr["ConfirmResourceKey"]);
            this.Order = Convert.ToInt32(dr["Order"]);
            this.APICall = dr["APICall"].ToString();

            // add audit column data
            this.FillInternal(dr);
        }
    }
}
