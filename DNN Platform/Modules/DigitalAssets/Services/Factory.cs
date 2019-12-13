using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using DotNetNuke.ExtensionPoints;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers;

namespace DotNetNuke.Modules.DigitalAssets.Services
{
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
                var dac = controllers.SingleOrDefault(c => c.Metadata.Edition == "PE");
                return dac != null ? dac.Value : controllers.Single(c => c.Metadata.Edition == "CE").Value;
            }
        }
    }
}
