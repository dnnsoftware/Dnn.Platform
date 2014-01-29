#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.ComponentModel;

using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.Entities.Users
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      UserMembership
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserMembership class provides Business Layer model for the Users Membership
    /// related properties
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///     [cnurse]	12/22/2005	documented
    /// </history>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class UserMembership
    {
        private readonly UserInfo _user;
        private bool _objectHydrated;

        public UserMembership(UserInfo user)
        {
            Approved = true;
            _user = user;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the User is Approved
        /// </summary>
        /// <history>
        ///     [cnurse]	02/27/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool Approved { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the User's Creation Date
        /// </summary>
        /// <history>
        ///     [cnurse]	02/27/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public DateTime CreatedDate { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the User Whether is deleted.
        /// </summary>
        /// <history>
        ///     [cnurse]	02/27/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool IsDeleted { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the User Is Online
        /// </summary>
        /// <history>
        ///     [cnurse]	02/27/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool IsOnLine { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Last Activity Date of the User
        /// </summary>
        /// <history>
        ///     [cnurse]	02/27/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public DateTime LastActivityDate { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Last Lock Out Date of the User
        /// </summary>
        /// <history>
        ///     [cnurse]	02/27/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public DateTime LastLockoutDate { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Last Login Date of the User
        /// </summary>
        /// <history>
        ///     [cnurse]	02/27/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public DateTime LastLoginDate { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Last Password Change Date of the User
        /// </summary>
        /// <history>
        ///     [cnurse]	02/27/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public DateTime LastPasswordChangeDate { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the user is locked out
        /// </summary>
        /// <history>
        ///     [cnurse]	02/27/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool LockedOut { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the User's Password
        /// </summary>
        /// <history>
        ///     [cnurse]	02/27/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Password { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the User's Password Answer
        /// </summary>
        /// <history>
        ///     [cnurse]	08/04/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string PasswordAnswer { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the User's Password Confirm value
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string PasswordConfirm { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the User's Password Question
        /// </summary>
        /// <history>
        ///     [cnurse]	02/27/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string PasswordQuestion { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets a flag that determines whether the password should be updated
        /// </summary>
        /// <history>
        ///     [cnurse]	03/14/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool UpdatePassword { get; set; }

        [Obsolete("Deprecated in DNN 5.1")]
        public UserMembership()
        {
            Approved = true;
            _user = new UserInfo();
        }

        [Obsolete("Deprecated in DNN 5.1")]
        [Browsable(false)]
        public string Email
        {
            get
            {
                return _user.Email;
            }
            set
            {
                _user.Email = value;
            }
        }

        [Obsolete("Deprecated in DNN 5.1")]
        [Browsable(false)]
        public bool ObjectHydrated
        {
            get
            {
                return _objectHydrated;
            }
            set
            {
                _objectHydrated = true;
            }
        }

        [Obsolete("Deprecated in DNN 5.1")]
        [Browsable(false)]
        public string Username
        {
            get
            {
                return _user.Username;
            }
            set
            {
                _user.Username = value;
            }
        }
    }
}
