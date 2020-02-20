// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Services.Social.Notifications
{
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
        /// The notification type identifier.
        /// </summary>
        [XmlAttribute]
        public int NotificationTypeId
        {
            get
            {
                return _notificationTypeId;
            }
            set
            {
                _notificationTypeId = value;
            }
        }

        /// <summary>
        /// The notification type name.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// The notification type description.
        /// </summary>
        [XmlAttribute]
        public string Description { get; set; }

        /// <summary>
        /// The amount of time to add to the creation date of the message to calculate the expiration date.
        /// </summary>
        /// <remarks>
        /// Minutes precision. Seconds won't be considered.
        /// </remarks>
        [XmlAttribute]
        public TimeSpan TimeToLive { get; set; }

        /// <summary>
        /// If the message type is related to a specific module, this field is used to localize actions by getting the resource file from the module folder.
        /// </summary>
        /// <remarks>
        /// The resource file used will be SharedResources by convention.
        /// </remarks>
        [XmlAttribute]
        public int DesktopModuleId
        {
            get
            {
                return _desktopModuleId;
            }
            set
            {
                _desktopModuleId = value;
            }
        }

        /// <summary>
        /// Is this of a Task type. Default is false.
        /// </summary>
        /// <remarks>
        /// Tasks are primarily notifications where an action must be taken. Dismiss is usually not enough.
        /// </remarks>
        [XmlAttribute]
        public bool IsTask { get; set; }

        #region Implementation of IHydratable

        /// <summary>
        /// IHydratable.KeyID.
        /// </summary>
        [XmlIgnore]
        public int KeyID
        {
            get { return NotificationTypeId; }
            set { NotificationTypeId = value; }
        }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            NotificationTypeId = Convert.ToInt32(dr["NotificationTypeID"]);
            Name = dr["Name"].ToString();
            Description = Null.SetNullString(dr["Description"]);
            var timeToLive = Null.SetNullInteger(dr["TTL"]);
            if (timeToLive != Null.NullInteger)
            {
                TimeToLive = new TimeSpan(0, timeToLive, 0);
            }
            DesktopModuleId = Null.SetNullInteger(dr["DesktopModuleID"]);
            IsTask = Null.SetNullBoolean(dr["IsTask"]);

            //add audit column data
            FillInternal(dr);
        }

        #endregion
    }
}
