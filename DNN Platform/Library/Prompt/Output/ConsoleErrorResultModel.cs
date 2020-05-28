// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Prompt
{
    public class ConsoleErrorResultModel : ConsoleResultModel
    {
        public ConsoleErrorResultModel()
        {
            IsError = true;
            Output = Localization.GetString("Prompt_InvalidSyntax", Constants.DefaultPromptResourceFile, true);
        }

        public ConsoleErrorResultModel(string errMessage)
        {
            IsError = true;
            Output = errMessage;
        }
    }
}
