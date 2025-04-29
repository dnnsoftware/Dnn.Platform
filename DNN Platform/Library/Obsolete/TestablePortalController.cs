// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Internal;

using System;
using System.ComponentModel;

using DotNetNuke.Framework;
using DotNetNuke.Internal.SourceGenerators;

[EditorBrowsable(EditorBrowsableState.Never)]
[DnnDeprecated(7, 3, 0, "Please use PortalController instead", RemovalVersion = 10)]
public partial class TestablePortalController : ServiceLocator<IPortalController, TestablePortalController>
{
    /// <inheritdoc/>
    protected override Func<IPortalController> GetFactory()
    {
        return () => new PortalController();
    }
}
