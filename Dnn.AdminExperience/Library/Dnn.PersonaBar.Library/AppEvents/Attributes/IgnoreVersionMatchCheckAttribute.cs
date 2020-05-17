// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace Dnn.PersonaBar.Library.AppEvents.Attributes
{
    /// <summary>
    /// Attribute to be used to decorate IAppLifecycleEvent implementors.
    /// This will ignore the version match check that is performed to avoid load IAppLifecycleEvent implementors
    /// on assembly with a version different from the version of Evoq.Library
    /// </summary>
    /// <remarks>This has been added as workaround for microservices module and a jira (CONTENT-6958) has been
    /// created to review if the version match check can be removed in the future</remarks>
    public class IgnoreVersionMatchCheckAttribute : Attribute
    {
    }
}
