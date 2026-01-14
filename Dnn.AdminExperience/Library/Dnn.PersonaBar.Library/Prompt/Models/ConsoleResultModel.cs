// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Prompt.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Internal.SourceGenerators;

    using Newtonsoft.Json;

    /// <summary>Standard response object sent to client.</summary>
    [DnnDeprecated(9, 7, 0, "Moved to DotNetNuke.Prompt in the core library project")]
    public partial class ConsoleResultModel
    {
        // the returned result - text or HTML
        [JsonProperty(PropertyName = "output")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string Output;

        // is the output an error message?
        [JsonProperty(PropertyName = "isError")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool IsError;

        // is the Output HTML?
        [JsonProperty(PropertyName = "isHtml")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool IsHtml;

        // should the client reload after processing the command
        [JsonProperty(PropertyName = "mustReload")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool MustReload;

        // the response contains data to be formatted by the client
        [JsonProperty(PropertyName = "data")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public object Data;

        // optionally tell the client in what order the fields should be displayed
        [JsonProperty(PropertyName = "fieldOrder")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string[] FieldOrder;

        [JsonProperty(PropertyName = "pagingInfo")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public PagingInfo PagingInfo;

        [JsonProperty(PropertyName = "nextPageCommand")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string NextPageCommand;

        /// <summary>Initializes a new instance of the <see cref="ConsoleResultModel"/> class.</summary>
        public ConsoleResultModel()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ConsoleResultModel"/> class.</summary>
        /// <param name="output">The console output.</param>
        public ConsoleResultModel(string output)
        {
            this.Output = output;
        }

        [JsonProperty(PropertyName = "records")]
        public int Records { get; set; }
    }
}
