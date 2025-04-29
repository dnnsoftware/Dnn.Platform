// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Internal;

using System;

using DotNetNuke.Framework;

/// <summary>A service locator for <see cref="IGlobals"/>.</summary>
public class TestableGlobals : ServiceLocator<IGlobals, TestableGlobals>
{
    /// <inheritdoc/>
    protected override Func<IGlobals> GetFactory()
    {
        return () => new GlobalsImpl();
    }
}
