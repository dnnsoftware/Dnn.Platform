#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
