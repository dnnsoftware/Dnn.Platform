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
    /// Class:      NotificationType
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The NotificationType class describes a single notification type that can be associated to a message.
    /// This message could be a notification or a standard message sent between users.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class NotificationType : BaseEntityInfo, IHydratable
    {
        private int _notificationTypeId = -1;
        private int _desktopModuleId = -1;

        /// <summary>
        /// Gets or sets the notification type identifier.
        /// </summary>
        [XmlAttribute]
        public int NotificationTypeId
        {
            get
            {
                return this._notificationTypeId;
            }

            set
            {
                this._notificationTypeId = value;
            }
        }

        /// <summary>
        /// Gets or sets the notification type name.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the notification type description.
        /// </summary>
        [XmlAttribute]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the amount of time to add to the creation date of the message to calculate the expiration date.
        /// </summary>
        /// <remarks>
        /// Minutes precision. Seconds won't be considered.
        /// </remarks>
        [XmlAttribute]
        public TimeSpan TimeToLive { get; set; }

        /// <summary>
        /// Gets or sets if the message type is related to a specific module, this field is used to localize actions by getting the resource file from the module folder.
        /// </summary>
        /// <remarks>
        /// The resource file used will be SharedResources by convention.
        /// </remarks>
        [XmlAttribute]
        public int DesktopModuleId
        {
            get
            {
                return this._desktopModuleId;
            }

            set
            {
                this._desktopModuleId = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether is this of a Task type. Default is false.
        /// </summary>
        /// <remarks>
        /// Tasks are primarily notifications where an action must be taken. Dismiss is usually not enough.
        /// </remarks>
        [XmlAttribute]
        public bool IsTask { get; set; }

        /// <summary>
        /// Gets or sets iHydratable.KeyID.
        /// </summary>
        [XmlIgnore]
        public int KeyID
        {
            get { return this.NotificationTypeId; }
            set { this.NotificationTypeId = value; }
        }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            this.NotificationTypeId = Convert.ToInt32(dr["NotificationTypeID"]);
            this.Name = dr["Name"].ToString();
            this.Description = Null.SetNullString(dr["Description"]);
            var timeToLive = Null.SetNullInteger(dr["TTL"]);
            if (timeToLive != Null.NullInteger)
            {
                this.TimeToLive = new TimeSpan(0, timeToLive, 0);
            }

            this.DesktopModuleId = Null.SetNullInteger(dr["DesktopModuleID"]);
            this.IsTask = Null.SetNullBoolean(dr["IsTask"]);

            // add audit column data
            this.FillInternal(dr);
        }
    }
}
