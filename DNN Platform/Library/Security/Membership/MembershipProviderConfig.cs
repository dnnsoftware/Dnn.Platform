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

using System.ComponentModel;

using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.Security.Membership
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Security.Membership
    /// Class:      MembershipProviderConfig
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MembershipProviderConfig class provides a wrapper to the Membership providers
    /// configuration
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class MembershipProviderConfig
    {
		#region "Private Shared Members"
		
        private static readonly MembershipProvider memberProvider = MembershipProvider.Instance();
		
		#endregion
		
		#region "Public Shared Properties"
		
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the Provider Properties can be edited
        /// </summary>
        /// <returns>A Boolean</returns>
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
        /// Gets and sets the maximum number of invlaid attempts to login are allowed
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(8), Category("Password")]
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
        /// Gets and sets the Mimimum no of Non AlphNumeric characters required
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(5), Category("Password")]
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
        /// Gets and sets the Mimimum Password Length
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(4), Category("Password")]
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
        /// Gets and sets the window in minutes that the maxium attempts are tracked for
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(9), Category("Password")]
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
        /// Gets and sets the Password Format
        /// </summary>
        /// <returns>A PasswordFormat enumeration.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(1), Category("Password")]
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
        /// Gets and sets whether the Users's Password can be reset
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(3), Category("Password")]
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
        /// Gets and sets whether the Users's Password can be retrieved
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(2), Category("Password")]
        public static bool PasswordRetrievalEnabled
        {
            get
            {
                bool enabled = memberProvider.PasswordRetrievalEnabled;

                //If password format is hashed the password cannot be retrieved
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
        /// Gets and sets a Regular Expression that determines the strength of the password
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(7), Category("Password")]
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
        /// Gets and sets whether a Question/Answer is required for Password retrieval
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(6), Category("Password")]
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
        /// Gets and sets whether a Unique Email is required
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(0), Category("User")]
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
			
			#endregion
        }
    }
}
