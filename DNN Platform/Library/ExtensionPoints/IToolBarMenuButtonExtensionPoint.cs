// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IToolBarMenuButtonExtensionPoint : IToolBarButtonExtensionPoint
    {
        List<IMenuButtonItemExtensionPoint> Items { get; }

        string MenuCssClass { get; }
    }
}
