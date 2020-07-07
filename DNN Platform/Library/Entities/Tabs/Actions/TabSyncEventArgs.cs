// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.Actions
{
    using System;
    using System.Xml;

    public class TabSyncEventArgs : TabEventArgs
    {
        public XmlNode TabNode { get; set; }
    }
}
