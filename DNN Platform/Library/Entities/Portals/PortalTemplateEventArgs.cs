// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Entities.Portals
{
    public class PortalTemplateEventArgs : EventArgs
    {
        public int PortalId { get; set; }

        public string TemplatePath { get; set; }
    }
}
