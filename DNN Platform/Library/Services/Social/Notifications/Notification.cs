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
    /// Namespace:  DotNetNuke.Entities.Notifications
    /// Class:      Notification
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Notification class describes the a notification received by a user as a consecuence of an action
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class Notification : BaseEntityInfo, IHydratable
    {
        #region Private Properties

        private int _notificationID = -1;
        private string _displayDate;

        #endregion

        #region Public Properties

        /// <summary>
        /// NotificationID - The primary key
        /// </summary>
        [XmlAttribute]
        public int NotificationID
        {
            get
            {
                return _notificationID;
            }
            set
            {
                _notificationID = value;
            }
        }

        /// <summary>
        /// The message type identifier.
        /// </summary>
        [XmlAttribute]
        public int NotificationTypeID { get; set; }

        /// <summary>
        /// To list for the message. This information is saved for faster display of To list in the message
        /// </summary>
        [XmlAttribute]
        public string To { get; set; }

        /// <summary>
        /// Message From
        /// </summary>
        [XmlAttribute]
        public string From { get; set; }

        /// <summary>
        /// Message Subject
        /// </summary>
        [XmlAttribute]
        public string Subject { get; set; }

        /// <summary>
        /// Message body
        /// </summary>
        [XmlAttribute]
        public string Body { get; set; }

        /// <summary>
        /// Context set by creator of the notification
        /// </summary>
        [XmlAttribute]
        public string Context { get; set; }

        /// <summary>
        /// The UserID of the sender of the message
        /// </summary>
        [XmlAttribute]
        public int SenderUserID { get; set; }

        /// <summary>
        /// A pretty printed string with the time since the message was created
        /// </summary>
        [XmlAttribute]
        public string DisplayDate
        {
            get
            {
                if (string.IsNullOrEmpty(_displayDate))
                {
                    _displayDate = DateUtils.CalculateDateForDisplay(CreatedOnDate);
                }
                return _displayDate;
            }
        }

        /// <summary>
        /// For notifications, this field indicates when it has to be removed (or not displayed)
        /// </summary>
        [XmlAttribute]
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// IHydratable.KeyID.
        /// </summary>
        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return NotificationID;
            }
            set
            {
                NotificationID = value;
            }
        }

        /// <summary>
        /// Should this notification support a dismiss action
        /// </summary>
        [XmlAttribute]
        public bool IncludeDismissAction { get; set; }

        /// <summary>
        /// The notification is displayed on the UI as a toast notification
        /// </summary>
		[XmlAttribute]
		public bool SendToast { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Notification()
        {
            SendToast = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            NotificationID = Convert.ToInt32(dr["MessageID"]);
            NotificationTypeID = Convert.ToInt32(dr["NotificationTypeID"]);
            To = Null.SetNullString(dr["To"]);
            From = Null.SetNullString(dr["From"]);
            Subject = Null.SetNullString(dr["Subject"]);
            Body = Null.SetNullString(dr["Body"]);
            Context = Null.SetNullString(dr["Context"]);
            SenderUserID = Convert.ToInt32(dr["SenderUserID"]);
            ExpirationDate = Null.SetNullDateTime(dr["ExpirationDate"]);
            IncludeDismissAction = Null.SetNullBoolean(dr["IncludeDismissAction"]);

			var schema = dr.GetSchemaTable();
			if (schema != null)
			{
				if (schema.Select("ColumnName = 'SendToast'").Length > 0)
				{
					SendToast = Null.SetNullBoolean(dr["SendToast"]);
				}
				else
				{
                    SendToast = false;
				}
			}

            //add audit column data
            FillInternal(dr);
        }

        #endregion
    }
}
