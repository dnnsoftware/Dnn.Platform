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

using DotNetNuke.Common.Utilities;
using DotNetNuke.Security.Roles;


namespace DotNetNuke.Entities.Users
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      UserRoleInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserRoleInfo class provides Business Layer model for a User/Role
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class UserRoleInfo : RoleInfo
    {
        #region Public Properties

        public int UserRoleID { get; set; }

        public int UserID { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsOwner { get; set; }
        
        public bool IsTrialUsed { get; set; }

        public bool Subscribed { get; set; }

        #endregion

        public override void Fill(IDataReader dr)
        {
			//Fill base class properties
            base.Fill(dr);

			//Fill this class properties
            UserRoleID = Null.SetNullInteger(dr["UserRoleID"]);
            UserID = Null.SetNullInteger(dr["UserID"]);
            FullName = Null.SetNullString(dr["DisplayName"]);
            Email = Null.SetNullString(dr["Email"]);
            EffectiveDate = Null.SetNullDateTime(dr["EffectiveDate"]);
            ExpiryDate = Null.SetNullDateTime(dr["ExpiryDate"]);
            IsOwner = Null.SetNullBoolean(dr["IsOwner"]);
            IsTrialUsed = Null.SetNullBoolean(dr["IsTrialUsed"]);
            if (UserRoleID > Null.NullInteger)
            {
                Subscribed = true;
            }
        }
    }
}
