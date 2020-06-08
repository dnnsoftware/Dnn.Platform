// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Xml;

namespace DotNetNuke.Entities.Tabs.Actions
{
    public class TabSyncEventArgs : TabEventArgs
    {
        public XmlNode TabNode { get; set; }
    }
}
