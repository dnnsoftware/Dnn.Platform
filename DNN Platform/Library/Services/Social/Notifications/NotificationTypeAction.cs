// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
        /// The notification type action identifier.
        /// </summary>
        [XmlAttribute]
        public int NotificationTypeActionId
        {
            get
            {
                return _notificationTypeActionId;
            }
            set
            {
                _notificationTypeActionId = value;
            }
        }

        /// <summary>
        /// The notification type identifier.
        /// </summary>
        [XmlAttribute]
        public int NotificationTypeId { get; set; }

        /// <summary>
        /// The notification type action name resource key.
        /// </summary>
        [XmlAttribute]
        public string NameResourceKey { get; set; }

        /// <summary>
        /// The notification type action description resource key.
        /// </summary>
        [XmlAttribute]
        public string DescriptionResourceKey { get; set; }

        /// <summary>
        /// The notification type action confirmation resource key.
        /// </summary>
        [XmlAttribute]
        public string ConfirmResourceKey { get; set; }

        /// <summary>
        /// The notification type action order to be used while displaying the list of available actions.
        /// </summary>
        [XmlAttribute]
        public int Order { get; set; }

        /// <summary>
        /// The Service Framework URL to be called when the action is performed.
        /// </summary>
        [XmlAttribute]
        public string APICall { get; set; }

        #region Implementation of IHydratable

        /// <summary>
        /// IHydratable.KeyID.
        /// </summary>
        [XmlIgnore]
        public int KeyID
        {
            get { return NotificationTypeActionId; }
            set { NotificationTypeActionId = value; }
        }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            NotificationTypeActionId = Convert.ToInt32(dr["NotificationTypeActionID"]);
            NotificationTypeId = Convert.ToInt32(dr["NotificationTypeID"]);
            NameResourceKey = dr["NameResourceKey"].ToString();
            DescriptionResourceKey = Null.SetNullString(dr["DescriptionResourceKey"]);
            ConfirmResourceKey = Null.SetNullString(dr["ConfirmResourceKey"]);
            Order = Convert.ToInt32(dr["Order"]);
            APICall = dr["APICall"].ToString();

            //add audit column data
            FillInternal(dr);
        }

        #endregion
    }
}
