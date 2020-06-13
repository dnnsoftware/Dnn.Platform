// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Components
{
    using System;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Instrumentation;

    public class BusinessController : IUpgradeable
    {
        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(BusinessController));

        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "01.00.00":
                    break;
            }

            return "Success";
        }
    }
}
