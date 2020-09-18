// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Prompt.Models
{
    using System.IO;

    using Localization = DotNetNuke.Services.Localization.Localization;
    [Obsolete("Moved to DotNetNuke.Prompt in the core library project. Will be removed in DNN 11.", false)]
    public class ConsoleErrorResultModel : ConsoleResultModel
    {
        public ConsoleErrorResultModel()
        {
            this.IsError = true;
            this.Output = Localization.GetString("Prompt_InvalidSyntax", LocalResourcesFile, true);
        }

        public ConsoleErrorResultModel(string errMessage)
        {
            this.IsError = true;
            this.Output = errMessage;
        }

        private static string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/App_LocalResources/SharedResources.resx");
    }
}
