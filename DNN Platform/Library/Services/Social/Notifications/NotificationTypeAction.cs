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
