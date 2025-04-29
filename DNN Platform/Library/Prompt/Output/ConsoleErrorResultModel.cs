// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Prompt;

using DotNetNuke.Services.Localization;

/// <summary>This is used to return the results of a command error to the client.</summary>
public class ConsoleErrorResultModel : ConsoleResultModel
{
    /// <summary>Initializes a new instance of the <see cref="ConsoleErrorResultModel"/> class.</summary>
    public ConsoleErrorResultModel()
    {
        this.IsError = true;
        this.Output = Localization.GetString("Prompt_InvalidSyntax", Constants.DefaultPromptResourceFile, true);
    }

    /// <summary>Initializes a new instance of the <see cref="ConsoleErrorResultModel"/> class.</summary>
    /// <param name="errMessage"></param>
    public ConsoleErrorResultModel(string errMessage)
    {
        this.IsError = true;
        this.Output = errMessage;
    }
}
