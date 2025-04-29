// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Prompt;

using DotNetNuke.Abstractions.Prompt;
using Newtonsoft.Json;

/// <summary>This is used to return the results of the execution of a command to the client.</summary>
public class ConsoleResultModel : IConsoleResultModel
{
    /// <summary>Initializes a new instance of the <see cref="ConsoleResultModel"/> class.</summary>
    public ConsoleResultModel()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ConsoleResultModel"/> class.</summary>
    /// <param name="output"></param>
    public ConsoleResultModel(string output)
    {
        this.Output = output;
    }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "output")]
    public string Output { get; set; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "isError")]
    public bool IsError { get; set; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "isHtml")]
    public bool IsHtml { get; set; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "mustReload")]
    public bool MustReload { get; set; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "data")]
    public object Data { get; set; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "fieldOrder")]
    public string[] FieldOrder { get; set; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "pagingInfo")]
    public IPagingInfo PagingInfo { get; set; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "nextPageCommand")]
    public string NextPageCommand { get; set; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "records")]
    public int Records { get; set; }
}
