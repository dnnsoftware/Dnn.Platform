// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System;
using System.Linq;

using Cake.Frosting;

/// <summary>A cake task to create the <c>settings.local.json</c> file.</summary>
public sealed class CreateSettings : FrostingTask<Context>
{
    // Doesn't need to do anything as it's done automatically
}
