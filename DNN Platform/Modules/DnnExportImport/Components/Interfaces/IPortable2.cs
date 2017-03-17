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

using System;
using System.Runtime.InteropServices;
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
        /// A data structure representing a checkpoint for the export/import task. This can be used to tell the
        /// implementor where to resume it's operation if the job was interrupted previously.
        /// </summary>
        /// <remarks>It is up to each IPortable2 implementor to track its own stages and status values to
        /// properly export/import all of its items in/when and interruption occurs.</remarks>
        ExportImportChekpoint CheckPoint { get; set; }

        /// <summary>
        /// A callback to the export/import engine to check if the undergoing export/import process was cancelled.
        /// <para>The interface concrete classes should keep checking continuously for the cancellation flag to be true</para>
        /// <para>If the callback returns true, the IPortable2 implementations should stop any work they do and
        /// return control to the caller immediately or as soon as possible.</para>
        /// </summary>
        Func<ExportImportJob, bool> CheckCancelled { get; set; }

        /// <summary>
        /// Callback function to provide a checkpoint mechanism for an <see cref="IPortable2"/> implementation.
        /// The actual method shoul persist the reported checkpoint in the engine so if the process is interrupted,
        /// it can be resumed. If the reponse to calling this function was false, then the task should stop
        /// processing and return control immediately to the caller.
        /// </summary>
        /// <returns>True if the implementation to abort progress; false to continue.</returns>
        Func<IPortable2, bool> CheckPointStageCallback { get; set; }

        /// <summary>
        ///  Performs export operation of the object.
        /// </summary>
        void ExportData(ExportImportJob exportJob, ExportDto exportDto);

        /// <summary>
        ///  Performs import operation of the object.
        /// </summary>
        void ImportData(ExportImportJob importJob, ExportDto exportDto);
    }
}