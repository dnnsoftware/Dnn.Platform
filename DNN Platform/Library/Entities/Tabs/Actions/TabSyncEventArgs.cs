// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Xml;

namespace DotNetNuke.Entities.Tabs.Actions
{
    public class TabSyncEventArgs : TabEventArgs
    {
        public XmlNode TabNode { get; set; }
    }
}
