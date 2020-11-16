// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Tokens
{
    /// <summary>
    /// Scope informs the property access classes about the planned usage of the token.
    /// </summary>
    /// <remarks>
    /// The result of a token replace operation depends on the current context, privacy settings
    /// and the current scope. The scope should be the lowest scope needed for the current purpose.
    /// The property access classes should evaluate and use the scope before returning a value.
    /// </remarks>
    public enum Scope
    {
        /// <summary>
        /// Only access to Date and Time
        /// </summary>
        NoSettings = 0,

        /// <summary>
        /// Tokens for Host, Portal, Tab (, Module), user name
        /// </summary>
        Configuration = 1,

        /// <summary>
        /// Configuration, Current User data and user data allowed for registered members
        /// </summary>
        DefaultSettings = 2,

        /// <summary>
        /// System notifications to users and adminstrators
        /// </summary>
        SystemMessages = 3,

        /// <summary>
        /// internal debugging, error messages, logs
        /// </summary>
        Debug = 4,
    }
}
