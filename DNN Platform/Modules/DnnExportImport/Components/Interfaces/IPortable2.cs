#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Models;
using Dnn.ExportImport.Components.Repository;
using DotNetNuke.UI.UserControls;

namespace Dnn.ExportImport.Components.Interfaces
{
    public interface IPortable2
    {
        /// <summary>
        /// Category name for the exportable module. For example: "ASSETS".
        /// </summary>
        string Category { get; }

        /// <summary>
        /// Category name for the parent exportable module. For example: "USERS".
        /// If this is null, then the category is a top parent.
        /// </summary>
        string ParentCategory { get; }

        /// <summary>
        /// A priority for exporting/importing the object. Objects with higher 
        /// priority are exported/imported first. Highest priority is 0.
        /// </summary>
        uint Priority { get; }

        /// <summary>
        /// Whether the export/import operation can be cancelled during the operation.
        /// </summary>
        bool CanCancel { get; }

        /// <summary>
        /// Whether the import operation can be rolled back if interrupted/cancelled while in progress.
        /// </summary>
        bool CanRollback { get; }

        /// <summary>
        ///  Performs export operation of the object.
        /// </summary>
        void ExportData(ExportImportJob exportJob, IExportImportRepository repository, ExportImportResult result);

        /// <summary>
        ///  Performs import operation of the object.
        /// </summary>
        void ImportData(ExportImportJob importJob, ExportDto exporteDto, IExportImportRepository repository, ExportImportResult result);
    }
}