// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;

    /// <summary>
    /// This class defines the attribute that sets a property to be a required property.
    /// A step execution fails if a required property is not set by the time <see cref="IStep.Execute"/> is called.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class RequiredAttribute : Attribute
    {
    }
}
