// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Settings
{
    using System;

    /// <summary>
    /// When applied to a property this attribute persists the value of the property in the DNN ModuleSettings referenced by the ModuleId within the context.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    [Serializable]
    public class ModuleSettingAttribute : ParameterAttributeBase
    {}
}
