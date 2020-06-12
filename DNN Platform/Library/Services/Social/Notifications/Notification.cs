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
                return this._notificationID;
            }
            set
            {
                this._notificationID = value;
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
                if (string.IsNullOrEmpty(this._displayDate))
                {
                    this._displayDate = DateUtils.CalculateDateForDisplay(this.CreatedOnDate);
                }
                return this._displayDate;
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
                return this.NotificationID;
            }
            set
            {
                this.NotificationID = value;
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
            this.SendToast = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            this.NotificationID = Convert.ToInt32(dr["MessageID"]);
            this.NotificationTypeID = Convert.ToInt32(dr["NotificationTypeID"]);
            this.To = Null.SetNullString(dr["To"]);
            this.From = Null.SetNullString(dr["From"]);
            this.Subject = Null.SetNullString(dr["Subject"]);
            this.Body = Null.SetNullString(dr["Body"]);
            this.Context = Null.SetNullString(dr["Context"]);
            this.SenderUserID = Convert.ToInt32(dr["SenderUserID"]);
            this.ExpirationDate = Null.SetNullDateTime(dr["ExpirationDate"]);
            this.IncludeDismissAction = Null.SetNullBoolean(dr["IncludeDismissAction"]);

			var schema = dr.GetSchemaTable();
			if (schema != null)
			{
				if (schema.Select("ColumnName = 'SendToast'").Length > 0)
				{
					this.SendToast = Null.SetNullBoolean(dr["SendToast"]);
				}
				else
				{
                    this.SendToast = false;
				}
			}

            // add audit column data
            this.FillInternal(dr);
        }

        #endregion
    }
}
