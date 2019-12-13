// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Common.Utilities;
using System;
using System.Data;

namespace DotNetNuke.Entities.Users.Membership
{
    [Serializable]
    public class PasswordHistory : BaseEntityInfo
    {
        public int PasswordHistoryId { get; set; }
        public int UserId { get; set; }
        public string Password  { get; set; }
        public string PasswordSalt  { get; set; }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            this.PasswordHistoryId = Convert.ToInt32(dr["PasswordHistoryID"]);
            this.UserId = Null.SetNullInteger(dr["UserID"]);
            this.Password = (dr["Password"]).ToString();
            this.PasswordSalt = (dr["PasswordSalt"]).ToString();

            //add audit column data
            FillInternal(dr);
        }

    }
}
