// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Entities.Modules.Settings
{
    /// <summary>
    /// When applied to a property this attribute persists the value of the property in the DNN TabModuleSettings referenced by the TabModuleId within the context.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class TabModuleSettingAttribute : ParameterAttributeBase
    {
    }
}
