// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public enum SeverityEnum
    {
        /// <summary>Pass successfully.</summary>
        Pass = 0,

        /// <summary>Warning.</summary>
        Warning = 1,

        /// <summary>Failure.</summary>
        Failure = 2,

        /// <summary>Unable to verify.</summary>
        Unverified = 3,
    }
}
