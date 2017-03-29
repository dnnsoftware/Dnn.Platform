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
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Models;

namespace Dnn.ExportImport.Components.Services
{
    /// <summary>
    /// A abstract class specifying the ability to import and export site different areas.
    /// </summary>
    public abstract class BasePortableService
    {
        /// <summary>
        /// An object to record the export/import progress information.
        /// This is set by the export/import engine.
        /// </summary>
        public ExportImportResult Result { get; set; }

        /// <summary>
        /// A repository to store exported items in and to retrieve from upon import.
        /// </summary>
        public IExportImportRepository Repository { get; set; }

        /// <summary>
        /// A data structure representing a checkpoint for the export/import task. This can be used to tell the
        /// implementor where to resume it's operation if the job was interrupted previously.
        /// </summary>
        /// <remarks>It is up to each BasePortableService implementor to track its own stages and status values to
        /// properly export/import all of its items in/when and interruption occurs.</remarks>
        public ExportImportChekpoint CheckPoint { get; set; }

        /// <summary>
        /// A callback to the export/import engine to check if the undergoing export/import process was cancelled.
        /// <para>The interface concrete classes should keep checking continuously for the cancellation flag to be true</para>
        /// <para>If the callback returns true, the BasePortableService implementations should stop any work they do and
        /// return control to the caller immediately or as soon as possible.</para>
        /// </summary>
        public Func<ExportImportJob, bool> CheckCancelled { get; set; }

        /// <summary>
        /// Callback function to provide a checkpoint mechanism for an <see cref="BasePortableService"/> implementation.
        /// The actual method shoul persist the reported checkpoint in the engine so if the process is interrupted,
        /// it can be resumed. If the reponse to calling this function was false, then the task should stop
        /// processing and return control immediately to the caller.
        /// </summary>
        /// <returns>True if the implementation to abort progress; false to continue.</returns>
        public Func<BasePortableService, bool> CheckPointStageCallback { get; set; }

        // The following properties and methods must be overriden in descendant classes

        /// <summary>
        /// Category name for the exportable module. For example: "ASSETS".
        /// </summary>
        public abstract string Category { get; }

        /// <summary>
        /// Category name for the parent exportable module. For example: "USERS".
        /// If this is null, then the category is a top parent. If this is not null,
        /// then this category will be included automatically when the parent is included.
        /// </summary>
        public abstract string ParentCategory { get; }

        /// <summary>
        /// A priority for exporting/importing the object. Objects with higher
        /// priority are exported/imported first. Highest priority is 0.
        /// </summary>
        public abstract uint Priority { get; }

        public abstract void ExportData(ExportImportJob exportJob, ExportDto exportDto);
        public abstract void ImportData(ExportImportJob importJob, ImportDto importDto);
        public abstract int GetImportTotal();
    }
}