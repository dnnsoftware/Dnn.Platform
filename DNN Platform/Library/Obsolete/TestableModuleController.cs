// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Internal;

using System;
using System.ComponentModel;

using DotNetNuke.Framework;
using DotNetNuke.Internal.SourceGenerators;

[EditorBrowsable(EditorBrowsableState.Never)]
[DnnDeprecated(7, 3, 0, "Please use ModuleController instead", RemovalVersion = 10)]
public partial class TestableModuleController : ServiceLocator<IModuleController, TestableModuleController>, IModuleController
{
    /// <inheritdoc/>
    public ModuleInfo GetModule(int moduleId, int tabId)
    {
        return ModuleController.Instance.GetModule(moduleId, tabId, false);
    }

    /// <inheritdoc/>
    public void UpdateModuleSetting(int moduleId, string settingName, string settingValue)
    {
        ModuleController.Instance.UpdateModuleSetting(moduleId, settingName, settingValue);
    }

    /// <inheritdoc/>
    public void UpdateTabModuleSetting(int tabModuleId, string settingName, string settingValue)
    {
        ModuleController.Instance.UpdateTabModuleSetting(tabModuleId, settingName, settingValue);
    }

    /// <inheritdoc/>
    protected override Func<IModuleController> GetFactory()
    {
        return () => new TestableModuleController();
    }
}
