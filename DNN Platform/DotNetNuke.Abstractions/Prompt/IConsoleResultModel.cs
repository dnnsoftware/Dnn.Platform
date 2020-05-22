// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 

namespace DotNetNuke.Abstractions.Prompt
{
    public interface IConsoleResultModel
    {
        // the returned result - text or HTML
        string Output { get; set; }
        // is the output an error message?
        bool IsError { get; set; }
        // is the Output HTML?
        bool IsHtml { get; set; }
        // should the client reload after processing the command
        bool MustReload { get; set; }
        // the response contains data to be formatted by the client
        object Data { get; set; }
        // optionally tell the client in what order the fields should be displayed
        string[] FieldOrder { get; set; }
        IPagingInfo PagingInfo { get; set; }
        string NextPageCommand { get; set; }
        int Records { get; set; }
    }
}