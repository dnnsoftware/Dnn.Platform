// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Prompt
{
    /// <summary>
    /// This is used to return the results of the execution of a command to the client
    /// </summary>
    public interface IConsoleResultModel
    {
        /// <summary>
        /// The returned result - text or HTML 
        /// </summary>
        string Output { get; set; }
        /// <summary>
        /// Is the output an error message?
        /// </summary>
        bool IsError { get; set; }
        /// <summary>
        /// Let the client know if the output is HTML or not
        /// </summary>
        bool IsHtml { get; set; }
        /// <summary>
        /// Should the client reload after processing the command
        /// </summary>
        bool MustReload { get; set; }
        /// <summary>
        /// The response contains data to be formatted by the client
        /// </summary>
        object Data { get; set; }
        /// <summary>
        /// Optionally tell the client in what order the fields should be displayed
        /// </summary>
        string[] FieldOrder { get; set; }
        /// <summary>
        /// Information about paging of data. This allows the client to prompt the user
        /// to load the next page of data.
        /// </summary>
        IPagingInfo PagingInfo { get; set; }
        /// <summary>
        /// Command to be used to display the next page of data. This is set in the
        /// WebAPI handler.
        /// </summary>
        string NextPageCommand { get; set; }
        /// <summary>
        /// Nr of records retrieved (for this page).
        /// </summary>
        int Records { get; set; }
    }
}
