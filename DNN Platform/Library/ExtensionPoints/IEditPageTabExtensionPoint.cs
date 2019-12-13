// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.ExtensionPoints
{
    public interface IEditPageTabExtensionPoint : IUserControlExtensionPoint
    {
        string EditPageTabId { get; }
        string CssClass { get; }
        string Permission { get; }
    }
}
