// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs;

using System;

public class TabException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="TabException"/> class.</summary>
    /// <param name="tabId"></param>
    /// <param name="message"></param>
    public TabException(int tabId, string message)
        : base(message)
    {
        this.TabId = tabId;
    }

    public int TabId { get; private set; }
}
