// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.ExtensionPoints
{
    public interface IToolBarMenuButtonExtensionPoint: IToolBarButtonExtensionPoint
    {
        List<IMenuButtonItemExtensionPoint> Items { get; }
        string MenuCssClass { get; }
    }
}
