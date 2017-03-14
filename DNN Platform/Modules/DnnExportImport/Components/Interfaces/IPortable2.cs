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

using System.Threading;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Models;

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
        /// An object to record the export/import progress information.
        /// This is set by the export/import engine.
        /// </summary>
        ExportImportResult Result { get; set; }

        /// <summary>
        /// A repository to store exported items in and to retrieve from upon import.
        /// </summary>
        IExportImportRepository Repository { get; set; }

        /// <summary>
        /// A token to be set by the export/import engine to request the cancellation of the
        /// undergoing export/import process.
        /// <para>Each running export/import process should check this token to allow for cancellation.</para>
        /// <para>The interface concrete classes should keep checking continuously for the cancellation flag
        /// using "CancellationToken.IsCancellationRequested" to be true.</para>
        /// <para>If the "CancellationToken.Cancel()" method is called, the IPortable2 implementations should
        /// stop any work they do and return to the caller as soon as possible.</para>
        /// </summary>
        CancellationToken CancellationToken { get; set; }

        /// <summary>
        ///  Performs export operation of the object.
        /// </summary>
        void ExportData(ExportImportJob exportJob, ExportDto exportDto);

        /// <summary>
        ///  Performs import operation of the object.
        /// </summary>
        void ImportData(ExportImportJob importJob, ExportDto exporteDto);
    }
}