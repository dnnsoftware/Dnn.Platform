// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint
{
    public class PropertiesTabContentControl : PortalModuleBase
    {
        public delegate void ItemUpdatedHandler();

        public event ItemUpdatedHandler OnItemUpdated;

        public virtual void ItemUpdated()
        {
            if (OnItemUpdated != null)
            {
                OnItemUpdated();
            }
        }

        public virtual void DataBindItem()
        {            
        }        
    }
}
