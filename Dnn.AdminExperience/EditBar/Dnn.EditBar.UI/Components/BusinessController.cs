// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Components;

using DotNetNuke.Entities.Modules;

public class BusinessController : IUpgradeable
{
    /// <inheritdoc/>
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
