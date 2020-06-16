// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Web.UI;

    public interface IJavaScriptUtils
    {
        /// <summary>
        /// Registers a javascript variable in a page with its value.
        /// </summary>
        /// <param name="variableName">Variable name and also the name of the registered code.</param>
        /// <param name="value">Object to be assigned to the variable.</param>
        /// <param name="page">Page where the varialbe will be registered.</param>
        /// <param name="type">Type.</param>
        void RegisterJavascriptVariable(string variableName, object value, Page page, Type type);
    }
}
