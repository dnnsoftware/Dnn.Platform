// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Prompt
{
    using DotNetNuke.Abstractions.Prompt;
    using Newtonsoft.Json;

    public class ConsoleResultModel : IConsoleResultModel
    {
        // the returned result - text or HTML

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

        // is the output an error message?

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "isError")]
        public bool IsError { get; set; }

        // is the Output HTML?

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "isHtml")]
        public bool IsHtml { get; set; }

        // should the client reload after processing the command

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "mustReload")]
        public bool MustReload { get; set; }

        // the response contains data to be formatted by the client

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

        // optionally tell the client in what order the fields should be displayed

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
}
