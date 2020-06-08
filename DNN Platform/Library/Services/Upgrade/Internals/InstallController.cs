﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;

using DotNetNuke.Framework;
using DotNetNuke.Services.Upgrade.InternalController;

#endregion

namespace DotNetNuke.Services.Upgrade.Internals
{
	/// <summary>
	/// Business Layer to manage Messaging. Also contains CRUD methods.
	/// </summary>
    public class InstallController : ServiceLocator<IInstallController, InstallController>
    {
        protected override Func<IInstallController> GetFactory()
        {
            return () => new InstallControllerImpl();
        }
    }
}
