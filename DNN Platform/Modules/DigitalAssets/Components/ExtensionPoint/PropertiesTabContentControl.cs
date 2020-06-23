// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint
{
    using System;

    using DotNetNuke.Entities.Modules;

    public class PropertiesTabContentControl : PortalModuleBase
    {
        public delegate void ItemUpdatedHandler();

        public event ItemUpdatedHandler OnItemUpdated;

        public virtual void ItemUpdated()
        {
            if (this.OnItemUpdated != null)
            {
                this.OnItemUpdated();
            }
        }

        public virtual void DataBindItem()
        {
        }
    }
}
