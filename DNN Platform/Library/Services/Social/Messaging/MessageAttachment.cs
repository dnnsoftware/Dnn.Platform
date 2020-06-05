﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Social.Messaging
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Messaging
    /// Class:      MessageAttachment
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MessageAttachment class describes the content attachments associated with a message
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class MessageAttachment : BaseEntityInfo, IHydratable
    {
        private int _messageattachmentID = -1;

        /// <summary>
        /// MessageAttachmentID - The primary key
        /// </summary>
        [XmlAttribute]
        public int MessageAttachmentID
        {
            get
            {
                return this._messageattachmentID;
            }
            set
            {
                this._messageattachmentID = value;
            }
        }

        /// <summary>
        /// MessageID of the message that contains this attachment
        /// </summary>
        [XmlAttribute]
        public int MessageID { get; set; }

        /// <summary>
        /// The FileID of the attachment (what will be used against the Files table to provide the attachment)
        /// </summary>
        [XmlAttribute]
        public int FileID { get; set; }

      
        /// <summary>
        /// IHydratable.KeyID.
        /// </summary>
        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return this.MessageAttachmentID;
            }
            set
            {
                this.MessageAttachmentID = value;
            }
        }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            this.MessageAttachmentID = Convert.ToInt32(dr["MessageAttachmentID"]);
            this.MessageID = Convert.ToInt32(dr["MessageID"]);
            this.FileID = Convert.ToInt32(dr["FileID"]);
            
            //add audit column data
            this.FillInternal(dr);
            
        }
    }
}
