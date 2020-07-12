// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.ImprovementsProgram
{
    using System;

    [Flags]
    internal enum RolesEnum
    {
        None = 0,
        Host = 1,
        Admin = 2,
        CommunityManager = 4,
        ContentManager = 8,
        ContentEditor = 16,
    }
}
