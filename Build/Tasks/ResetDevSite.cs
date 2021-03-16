﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Linq;

    using Cake.Frosting;

    /// <summary>A cake task to reset a local dev site.</summary>
    [Dependency(typeof(BuildToTempFolder))]
    [Dependency(typeof(CopyToDevSite))]
    [Dependency(typeof(CopyWebConfigToDevSite))]
    public sealed class ResetDevSite : FrostingTask<Context>
    {
    }
}
