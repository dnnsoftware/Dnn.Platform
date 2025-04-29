// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs;

public class TabExistsException : TabException
{
    /// <summary>Initializes a new instance of the <see cref="TabExistsException"/> class.</summary>
    /// <param name="tabId"></param>
    /// <param name="message"></param>
    public TabExistsException(int tabId, string message)
        : base(tabId, message)
    {
    }
}
