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
    /// Class:      MessageRecipient
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MessageRecipient class is used to store the details of all recipients of a particular message
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class MessageRecipient : BaseEntityInfo, IHydratable
    {
        private int _recipientID = -1;

        /// <summary>
        /// RecipientID - The primary key
        /// </summary>
        [XmlAttribute]
        public int RecipientID
        {
            get
            {
                return _recipientID;
            }
            set
            {
                _recipientID = value;
            }
        }

        /// <summary>
        /// The messageID of who sent the message to this recipient
        /// </summary>
        [XmlAttribute]
        public int MessageID { get; set; }

        /// <summary>
        /// The UserID of the user receiving the message
        /// </summary>
        [XmlAttribute]
        public int UserID { get; set; }

        /// <summary>
        /// Is Message read. True: Yes, False: No.
        /// </summary>
        [XmlAttribute]
        public bool Read { get; set; }


        /// <summary>
        /// Is Message archived. True: Yes, False: No.
        /// </summary>
        [XmlAttribute]
        public bool Archived { get; set; }
       
        /// <summary>
        /// IHydratable.KeyID.
        /// </summary>
        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return this.RecipientID;
            }
            set
            {
                this.RecipientID = value;
            }
        }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            this.RecipientID = Convert.ToInt32(dr["RecipientID"]);
            this.MessageID = Convert.ToInt32(dr["MessageID"]);
            this.UserID = Convert.ToInt32(dr["UserID"]);
            this.Archived = Null.SetNullBoolean(dr["Archived"]);
            this.Read = Null.SetNullBoolean(dr["Read"]);
            
            //add audit column data
            FillInternal(dr);
            
        }
    }
}
