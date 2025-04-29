// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Membership;

using System.ComponentModel;

using DotNetNuke.UI.WebControls;

/// <summary>The MembershipProviderConfig class provides a wrapper to the Membership providers configuration.</summary>
public class MembershipProviderConfig
{
    private static readonly MembershipProvider MemberProvider = MembershipProvider.Instance();

    /// <summary>Gets a value indicating whether the Provider Properties can be edited.</summary>
    [Browsable(false)]
    public static bool CanEditProviderProperties
    {
        get
        {
            return MemberProvider.CanEditProviderProperties;
        }
    }

    /// <summary>Gets or sets the maximum number of invalid attempts to login are allowed.</summary>
    [SortOrder(8)]
    [Category("Password")]
    public static int MaxInvalidPasswordAttempts
    {
        get
        {
            return MemberProvider.MaxInvalidPasswordAttempts;
        }

        set
        {
            MemberProvider.MaxInvalidPasswordAttempts = value;
        }
    }

    /// <summary>Gets or sets the Mimimum no of Non AlphNumeric characters required.</summary>
    /// <returns>An Integer.</returns>
    [SortOrder(5)]
    [Category("Password")]
    public static int MinNonAlphanumericCharacters
    {
        get
        {
            return MemberProvider.MinNonAlphanumericCharacters;
        }

        set
        {
            MemberProvider.MinNonAlphanumericCharacters = value;
        }
    }

    /// <summary>Gets or sets the Mimimum Password Length.</summary>
    /// <returns>An Integer.</returns>
    [SortOrder(4)]
    [Category("Password")]
    public static int MinPasswordLength
    {
        get
        {
            return MemberProvider.MinPasswordLength;
        }

        set
        {
            MemberProvider.MinPasswordLength = value;
        }
    }

    /// <summary>Gets or sets the window in minutes that the maxium attempts are tracked for.</summary>
    /// <returns>A Boolean.</returns>
    [SortOrder(9)]
    [Category("Password")]
    public static int PasswordAttemptWindow
    {
        get
        {
            return MemberProvider.PasswordAttemptWindow;
        }

        set
        {
            MemberProvider.PasswordAttemptWindow = value;
        }
    }

    /// <summary>Gets or sets the Password Format.</summary>
    /// <returns>A PasswordFormat enumeration.</returns>
    [SortOrder(1)]
    [Category("Password")]
    public static PasswordFormat PasswordFormat
    {
        get
        {
            return MemberProvider.PasswordFormat;
        }

        set
        {
            MemberProvider.PasswordFormat = value;
        }
    }

    /// <summary>Gets or sets a value indicating whether the Users's Password can be reset.</summary>
    /// <returns>A Boolean.</returns>
    [SortOrder(3)]
    [Category("Password")]
    public static bool PasswordResetEnabled
    {
        get
        {
            return MemberProvider.PasswordResetEnabled;
        }

        set
        {
            MemberProvider.PasswordResetEnabled = value;
        }
    }

    /// <summary>Gets or sets a value indicating whether the Users's Password can be retrieved.</summary>
    /// <returns>A Boolean.</returns>
    [SortOrder(2)]
    [Category("Password")]
    public static bool PasswordRetrievalEnabled
    {
        get
        {
            bool enabled = MemberProvider.PasswordRetrievalEnabled;

            // If password format is hashed the password cannot be retrieved
            if (MemberProvider.PasswordFormat == PasswordFormat.Hashed)
            {
                enabled = false;
            }

            return enabled;
        }

        set
        {
            MemberProvider.PasswordRetrievalEnabled = value;
        }
    }

    /// <summary>Gets or sets a Regular Expression that determines the strength of the password.</summary>
    /// <returns>A String.</returns>
    [SortOrder(7)]
    [Category("Password")]
    public static string PasswordStrengthRegularExpression
    {
        get
        {
            return MemberProvider.PasswordStrengthRegularExpression;
        }

        set
        {
            MemberProvider.PasswordStrengthRegularExpression = value;
        }
    }

    /// <summary>Gets or sets a value indicating whether a Question/Answer is required for Password retrieval.</summary>
    /// <returns>A Boolean.</returns>
    [SortOrder(6)]
    [Category("Password")]
    public static bool RequiresQuestionAndAnswer
    {
        get
        {
            return MemberProvider.RequiresQuestionAndAnswer;
        }

        set
        {
            MemberProvider.RequiresQuestionAndAnswer = value;
        }
    }

    /// <summary>Gets or sets a value indicating whether a Unique Email is required.</summary>
    /// <returns>A Boolean.</returns>
    [SortOrder(0)]
    [Category("User")]
    public static bool RequiresUniqueEmail
    {
        get
        {
            return MemberProvider.RequiresUniqueEmail;
        }

        set
        {
            MemberProvider.RequiresUniqueEmail = value;
        }
    }
}
