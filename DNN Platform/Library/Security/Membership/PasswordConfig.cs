// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Membership
{
    using System.ComponentModel;

    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.UI.WebControls;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Security.Membership
    /// Class:      PasswordConfig
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PasswordConfig class provides a wrapper any Portal wide Password Settings.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class PasswordConfig
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Password Expiry time in days.
        /// </summary>
        /// <returns>An integer.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(0)]
        [Category("Password")]
        public static int PasswordExpiry
        {
            get
            {
                return Host.PasswordExpiry;
            }

            set
            {
                HostController.Instance.Update("PasswordExpiry", value.ToString());
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the a Reminder time in days (to remind the user that theire password
        /// is about to expire.
        /// </summary>
        /// <returns>An integer.</returns>
        /// -----------------------------------------------------------------------------
        [SortOrder(1)]
        [Category("Password")]
        public static int PasswordExpiryReminder
        {
            get
            {
                return Host.PasswordExpiryReminder;
            }

            set
            {
                HostController.Instance.Update("PasswordExpiryReminder", value.ToString());
            }
        }
    }
}
