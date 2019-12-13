// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
