// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Mail.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Smtp OAuth controller interface.
    /// </summary>
    public interface ISmtpOAuthController
    {
        /// <summary>
        /// Get all the oauth providers.
        /// </summary>
        /// <returns>OAuth providers list.</returns>
        IList<ISmtpOAuthProvider> GetOAuthProviders();

        /// <summary>
        /// Get the oauth provider.
        /// </summary>
        /// <param name="name">provider name.</param>
        /// <returns>the oauth provider.</returns>
        ISmtpOAuthProvider GetOAuthProvider(string name);
    }
}
