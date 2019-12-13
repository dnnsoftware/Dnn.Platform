using System;

using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint
{
    public interface IFieldsControl
    {
        IDigitalAssetsController Controller { get; }

        ItemViewModel Item { get; }

        void SetController(IDigitalAssetsController damController);

        void SetItemViewModel(ItemViewModel itemViewModel);

        void SetPropertiesAvailability(bool availability);

        void SetPropertiesVisibility(bool visibility);

        void PrepareProperties();

        object SaveProperties();
    }
}
