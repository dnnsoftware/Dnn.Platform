// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Services.ImprovementsProgram
{
    [Flags]
    internal enum RolesEnum
    {
        None = 0,
        Host = 1,
        Admin = 2,
        CommunityManager = 4,
        ContentManager = 8,
        ContentEditor = 16
    }
}
