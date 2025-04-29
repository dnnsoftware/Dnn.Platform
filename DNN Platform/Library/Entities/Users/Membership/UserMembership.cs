// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users;

using System;

/// Project:    DotNetNuke
/// Namespace:  DotNetNuke.Entities.Users
/// Class:      UserMembership
/// <summary>
/// The UserMembership class provides Business Layer model for the Users Membership
/// related properties.
/// </summary>
[Serializable]
public class UserMembership
{
    private readonly UserInfo user;
    private bool approved;

    /// <summary>Initializes a new instance of the <see cref="UserMembership"/> class.</summary>
    public UserMembership()
        : this(new UserInfo())
    {
    }

    /// <summary>Initializes a new instance of the <see cref="UserMembership"/> class.</summary>
    /// <param name="user"></param>
    public UserMembership(UserInfo user)
    {
        this.approved = true;
        this.user = user;
    }

    /// <summary>Gets or sets a value indicating whether the User is Approved.</summary>
    public bool Approved
    {
        get
        {
            return this.approved;
        }

        set
        {
            if (!this.approved && value)
            {
                this.Approving = true;
            }

            this.approved = value;
        }
    }

    /// <summary>Gets or sets the User's Creation Date.</summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>Gets or sets a value indicating whether gets and sets the User Whether is deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Gets or sets a value indicating whether the User Is Online.</summary>
    [Obsolete("Deprecated in DotNetNuke 8.0.0. Other solutions exist outside of the DNN Platform. Scheduled for removal in v11.0.0.")]
    public bool IsOnLine { get; set; }

    /// <summary>Gets or sets the Last Activity Date of the User.</summary>
    public DateTime LastActivityDate { get; set; }

    /// <summary>Gets or sets the Last Lock Out Date of the User.</summary>
    public DateTime LastLockoutDate { get; set; }

    /// <summary>Gets or sets the Last Login Date of the User.</summary>
    public DateTime LastLoginDate { get; set; }

    /// <summary>Gets or sets the Last Password Change Date of the User.</summary>
    public DateTime LastPasswordChangeDate { get; set; }

    /// <summary>Gets or sets a value indicating whether the user is locked out.</summary>
    public bool LockedOut { get; set; }

    /// <summary>Gets or sets the User's Password.</summary>
    public string Password { get; set; }

    /// <summary>Gets or sets the User's Password Answer.</summary>
    public string PasswordAnswer { get; set; }

    /// <summary>Gets or sets the User's Password Confirm value.</summary>
    public string PasswordConfirm { get; set; }

    /// <summary>Gets or sets the User's Password Question.</summary>
    public string PasswordQuestion { get; set; }

    /// <summary>Gets or sets a value indicating whether gets and sets a flag that determines whether the password should be updated.</summary>
    public bool UpdatePassword { get; set; }

    internal bool Approving { get; private set; }

    internal void ConfirmApproved()
    {
        this.Approving = false;
    }
}
