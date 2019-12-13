// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Web.UI;

namespace DotNetNuke.Common.Utilities
{
    public interface IJavaScriptUtils
    {
        /// <summary>
        /// Registers a javascript variable in a page with its value
        /// </summary>
        /// <param name="variableName">Variable name and also the name of the registered code</param>
        /// <param name="value">Object to be assigned to the variable</param>
        /// <param name="page">Page where the varialbe will be registered</param>
        /// <param name="type">Type</param>
        void RegisterJavascriptVariable(string variableName, object value, Page page, Type type);
    }
}
