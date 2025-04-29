// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System;
using System.Linq;

using Cake.Common.Tools.NUnit;
using Cake.Frosting;

/// <summary>A cake task to run NUnit 3 tests.</summary>
/// <remarks>This task is not used (NUnit 3 is not used by DNN), you probably want <see cref="UnitTests"/>.</remarks>
[IsDependentOn(typeof(Build))]
public sealed class RunUnitTests : FrostingTask<Context>
{
    /// <inheritdoc/>
    public override void Run(Context context)
    {
        context.NUnit3(
            "./src/**/bin/" + context.BuildConfiguration + "/*.Test*.dll",
            new NUnit3Settings { NoResults = false });
    }
}
