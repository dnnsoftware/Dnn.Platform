// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Prompt
{
    /// <summary>This is used to return the results of the execution of a command to the client.</summary>
    public interface IConsoleResultModel
    {
        /// <summary>Gets or sets the returned result - text or HTML.</summary>
        string Output { get; set; }

        /// <summary>Gets or sets a value indicating whether the output message is an error message.</summary>
        bool IsError { get; set; }

        /// <summary>Gets or sets a value indicating whether output is HTML.</summary>
        /// <remarks>Let the client know if the output is HTML or not.</remarks>
        bool IsHtml { get; set; }

        /// <summary>Gets or sets a value indicating whether the prompt must reload.</summary>
        /// <remarks>Should the client reload after processing the command.</remarks>
        bool MustReload { get; set; }

        /// <summary>Gets or sets a list of data to be formatted by the client.</summary>
        /// <remarks>
        /// If the list contains a single item it will be displayed as a list of properties, otherwise the list will be displayed as a table.
        /// Each field will be rendered as plain text, unless it is wrapped in <see cref="IConsoleOutput"/>.
        /// </remarks>
        object Data { get; set; }

        /// <summary>Gets or sets the field order.</summary>
        /// <remarks>Optionally tell the client in what order the fields should be displayed.</remarks>
        string[] FieldOrder { get; set; }

        /// <summary>Gets or sets the <see cref="IPagingInfo"/>.</summary>
        /// <remarks>Information about paging of data. This allows the client to prompt the user to load the next page of data.</remarks>
        IPagingInfo PagingInfo { get; set; }

        /// <summary>Gets or sets the next page command.</summary>
        /// <remarks>Command to be used to display the next page of data. This is set in the WebAPI handler.</remarks>
        string NextPageCommand { get; set; }

        /// <summary>Gets or sets the number of records retrieved (for this page).</summary>
        int Records { get; set; }
    }
}
