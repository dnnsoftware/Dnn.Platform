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
