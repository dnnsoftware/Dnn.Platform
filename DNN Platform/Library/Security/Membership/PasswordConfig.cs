// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Membership
{
    using System.ComponentModel;
    using System.Globalization;

    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.UI.WebControls;

    /// <summary>The PasswordConfig class provides a wrapper any Portal wide Password Settings.</summary>
    public class PasswordConfig
    {
        /// <summary>Gets or sets the Password Expiry time in days.</summary>
        /// <returns>An integer.</returns>
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
                HostController.Instance.Update("PasswordExpiry", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Gets or sets the Reminder time in days (to remind the user that their password
        /// is about to expire).
        /// </summary>
        /// <returns>An integer.</returns>
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
                HostController.Instance.Update("PasswordExpiryReminder", value.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
