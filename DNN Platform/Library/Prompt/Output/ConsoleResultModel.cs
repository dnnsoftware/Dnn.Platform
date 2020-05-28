// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using DotNetNuke.Abstractions.Prompt;
using Newtonsoft.Json;

namespace DotNetNuke.Prompt
{
    public class ConsoleResultModel : IConsoleResultModel
    {
        // the returned result - text or HTML
        [JsonProperty(PropertyName = "output")]
        public string Output { get; set; }
        // is the output an error message?
        [JsonProperty(PropertyName = "isError")]
        public bool IsError { get; set; }
        // is the Output HTML?
        [JsonProperty(PropertyName = "isHtml")]
        public bool IsHtml { get; set; }
        // should the client reload after processing the command
        [JsonProperty(PropertyName = "mustReload")]
        public bool MustReload { get; set; }
        // the response contains data to be formatted by the client
        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }
        // optionally tell the client in what order the fields should be displayed
        [JsonProperty(PropertyName = "fieldOrder")]
        public string[] FieldOrder { get; set; }

        [JsonProperty(PropertyName = "pagingInfo")]
        public IPagingInfo PagingInfo { get; set; }

        [JsonProperty(PropertyName = "nextPageCommand")]
        public string NextPageCommand { get; set; }

        [JsonProperty(PropertyName = "records")]
        public int Records { get; set; }

        public ConsoleResultModel()
        {
        }

        public ConsoleResultModel(string output)
        {
            Output = output;
        }
    }
}
