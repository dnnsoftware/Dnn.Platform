// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint
{
    using System;

    using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
    using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;

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
