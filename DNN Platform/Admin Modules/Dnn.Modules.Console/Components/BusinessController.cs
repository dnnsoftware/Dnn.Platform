// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.Console.Components;

using System;

using DotNetNuke.Entities.Modules;

/// <summary>Implements the module's business controller interface(s).</summary>
public class BusinessController : IUpgradeable
{
    /// <inheritdoc/>
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
