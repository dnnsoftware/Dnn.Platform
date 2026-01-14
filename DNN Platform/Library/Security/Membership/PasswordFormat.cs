// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Membership
{
    public enum PasswordFormat
    {
        /// <summary>Passwords stored in plaintext.</summary>
        Clear = 0,

        /// <summary>Passwords stored via irreversible hashing.</summary>
        Hashed = 1,

        /// <summary>Passwords stored via reversible encrypted.</summary>
        Encrypted = 2,
    }
}
