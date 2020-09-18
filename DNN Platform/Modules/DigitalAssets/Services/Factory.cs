// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;

    using DotNetNuke.ExtensionPoints;
    using DotNetNuke.Modules.DigitalAssets.Components.Controllers;

    public class Factory
    {
#pragma warning disable 649
        [ImportMany]
        private IEnumerable<Lazy<IDigitalAssetsController, IDigitalAssetsControllerData>> controllers;
#pragma warning restore 649

        public Factory()
        {
            ExtensionPointManager.ComposeParts(this);
        }

        public IDigitalAssetsController DigitalAssetsController
        {
            get
            {
                var dac = this.controllers.SingleOrDefault(c => c.Metadata.Edition == "PE");
                return dac != null ? dac.Value : this.controllers.Single(c => c.Metadata.Edition == "CE").Value;
            }
        }
    }
}
