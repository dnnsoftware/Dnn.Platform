// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users.Membership
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;

    [Serializable]
    public class PasswordHistory : BaseEntityInfo
    {
        public int PasswordHistoryId { get; set; }

        public int UserId { get; set; }

        public string Password { get; set; }

        public string PasswordSalt { get; set; }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            this.PasswordHistoryId = Convert.ToInt32(dr["PasswordHistoryID"]);
            this.UserId = Null.SetNullInteger(dr["UserID"]);
            this.Password = dr["Password"].ToString();
            this.PasswordSalt = dr["PasswordSalt"].ToString();

            // add audit column data
            this.FillInternal(dr);
        }
    }
}
