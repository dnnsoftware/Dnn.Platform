// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Common
{
    using System;

    internal static class Extensions
    {
        internal static bool Equals(this Version version, Version compareTo)
        {
            return version.Major == compareTo.Major
                    && version.Minor == compareTo.Minor
                    && version.Build == compareTo.Build;
        }
    }
}
