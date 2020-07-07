// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Membership
{
    using System.ComponentModel;

    using DotNetNuke.UI.WebControls;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Security.Membership
    /// Class:      MembershipProviderConfig
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MembershipProviderConfig class provides a wrapper to the Membership providers
    /// configuration.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class MembershipProviderConfig
    {
        private static readonly MembershipProvider memberProvider = MembershipProvider.Instance();

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the Provider Properties can be edited.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public static bool CanEditProviderProperties
        {
            get
            {
                return memberProvider.CanEditProviderProperties;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the maximum number of invlaid attempts to login are allowed.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(8)]
        [Category("Password")]
        public static int MaxInvalidPasswordAttempts
        {
            get
            {
                return memberProvider.MaxInvalidPasswordAttempts;
            }

            set
            {
                memberProvider.MaxInvalidPasswordAttempts = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Mimimum no of Non AlphNumeric characters required.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(5)]
        [Category("Password")]
        public static int MinNonAlphanumericCharacters
        {
            get
            {
                return memberProvider.MinNonAlphanumericCharacters;
            }

            set
            {
                memberProvider.MinNonAlphanumericCharacters = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Mimimum Password Length.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(4)]
        [Category("Password")]
        public static int MinPasswordLength
        {
            get
            {
                return memberProvider.MinPasswordLength;
            }

            set
            {
                memberProvider.MinPasswordLength = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the window in minutes that the maxium attempts are tracked for.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(9)]
        [Category("Password")]
        public static int PasswordAttemptWindow
        {
            get
            {
                return memberProvider.PasswordAttemptWindow;
            }

            set
            {
                memberProvider.PasswordAttemptWindow = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Password Format.
        /// </summary>
        /// <returns>A PasswordFormat enumeration.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(1)]
        [Category("Password")]
        public static PasswordFormat PasswordFormat
        {
            get
            {
                return memberProvider.PasswordFormat;
            }

            set
            {
                memberProvider.PasswordFormat = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Users's Password can be reset.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(3)]
        [Category("Password")]
        public static bool PasswordResetEnabled
        {
            get
            {
                return memberProvider.PasswordResetEnabled;
            }

            set
            {
                memberProvider.PasswordResetEnabled = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Users's Password can be retrieved.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(2)]
        [Category("Password")]
        public static bool PasswordRetrievalEnabled
        {
            get
            {
                bool enabled = memberProvider.PasswordRetrievalEnabled;

                // If password format is hashed the password cannot be retrieved
                if (memberProvider.PasswordFormat == PasswordFormat.Hashed)
                {
                    enabled = false;
                }

                return enabled;
            }

            set
            {
                memberProvider.PasswordRetrievalEnabled = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets a Regular Expression that determines the strength of the password.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(7)]
        [Category("Password")]
        public static string PasswordStrengthRegularExpression
        {
            get
            {
                return memberProvider.PasswordStrengthRegularExpression;
            }

            set
            {
                memberProvider.PasswordStrengthRegularExpression = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether a Question/Answer is required for Password retrieval.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(6)]
        [Category("Password")]
        public static bool RequiresQuestionAndAnswer
        {
            get
            {
                return memberProvider.RequiresQuestionAndAnswer;
            }

            set
            {
                memberProvider.RequiresQuestionAndAnswer = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether a Unique Email is required.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(0)]
        [Category("User")]
        public static bool RequiresUniqueEmail
        {
            get
            {
                return memberProvider.RequiresUniqueEmail;
            }

            set
            {
                memberProvider.RequiresUniqueEmail = value;
            }
        }
    }
}
