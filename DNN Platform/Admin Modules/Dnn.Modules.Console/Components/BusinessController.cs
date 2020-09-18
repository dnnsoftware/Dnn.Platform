// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.Console.Components
{
    using System;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Services.Upgrade;

    /// <summary>
    ///
    /// </summary>
    public class BusinessController : IUpgradeable
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public string UpgradeModule(string version)
        {
            try
            {
                switch (version)
                {
                    case "08.00.00":

                        break;
                }

                return "Success";
            }
            catch (Exception)
            {
                return "Failed";
            }
        }
    }
}
