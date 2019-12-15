// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace Dnn.ExportImport.Components.Models
{
    public class ProgressToken
    {
        /// <summary>
        /// This can be used as a continuation token for the operation. It 
        /// is an identifier for next operation/step in import/export process.
        /// </summary>
        public string ContinuationKey { get; set; }

        /// <summary>
        ///  Indicates the completed percentage progress of export/import operation.
        /// </summary>
        public uint Progress { get; set; }
    }
}
