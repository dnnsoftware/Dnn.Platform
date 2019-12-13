// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.ExtensionPoints
{
    public interface IExtensionPoint
    {
        string Text { get; }
        string Icon { get; }
        int Order { get; }
    }
}
