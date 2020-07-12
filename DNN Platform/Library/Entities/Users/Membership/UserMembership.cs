// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users
{
    using System;
    using System.ComponentModel;

    using DotNetNuke.UI.WebControls;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      UserMembership
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserMembership class provides Business Layer model for the Users Membership
    /// related properties.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class UserMembership
    {
        private readonly UserInfo _user;
        private bool _approved;

        public UserMembership()
            : this(new UserInfo())
        {
        }

        public UserMembership(UserInfo user)
        {
            this._approved = true;
            this._user = user;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the User is Approved.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool Approved
        {
            get
            {
                return this._approved;
            }

            set
            {
                if (!this._approved && value)
                {
                    this.Approving = true;
                }

                this._approved = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the User's Creation Date.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public DateTime CreatedDate { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets the User Whether is deleted.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool IsDeleted { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the User Is Online.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public bool IsOnLine { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Last Activity Date of the User.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public DateTime LastActivityDate { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Last Lock Out Date of the User.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public DateTime LastLockoutDate { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Last Login Date of the User.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public DateTime LastLoginDate { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Last Password Change Date of the User.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public DateTime LastPasswordChangeDate { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the user is locked out.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool LockedOut { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the User's Password.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Password { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the User's Password Answer.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string PasswordAnswer { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the User's Password Confirm value.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string PasswordConfirm { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the User's Password Question.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string PasswordQuestion { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets a flag that determines whether the password should be updated.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool UpdatePassword { get; set; }

        internal bool Approving { get; private set; }

        internal void ConfirmApproved()
        {
            this.Approving = false;
        }
    }
}
