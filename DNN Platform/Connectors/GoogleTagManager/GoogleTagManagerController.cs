// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Analytics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using DotNetNuke.Instrumentation;

/// Namespace:  DotNetNuke.Services.Analytics
/// Module:     GoogleAnalytics
/// <summary>  Controller class definition for GoogleTagManager which handles upgrades.</summary>
public class GoogleTagManagerController
{
    /// <summary>  Handles module upgrades includes a new Google Tag Manager Asychronous script.</summary>
    /// <param name="version">Target Version number for the upgrade.</param>
    public void UpgradeModule(string version)
    {
    }
}
