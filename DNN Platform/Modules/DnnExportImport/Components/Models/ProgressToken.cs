// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Models
{
    public class ProgressToken
    {
        /// <summary>
        /// Gets or sets this can be used as a continuation token for the operation. It
        /// is an identifier for next operation/step in import/export process.
        /// </summary>
        public string ContinuationKey { get; set; }

        /// <summary>
        ///  Gets or sets indicates the completed percentage progress of export/import operation.
        /// </summary>
        public uint Progress { get; set; }
    }
}
