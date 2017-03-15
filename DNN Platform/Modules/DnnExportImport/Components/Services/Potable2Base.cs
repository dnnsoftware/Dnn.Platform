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
using System.Threading;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Models;

namespace Dnn.ExportImport.Components.Services
{
    public abstract class Potable2Base : IPortable2
    {
        public ExportImportResult Result { get; set; }
        public IExportImportRepository Repository { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public ExportImportChekpoint CheckPoint { get; set; }
        public Func<IPortable2, bool> CheckPointStageCallback { get; set; }

        // The following properties and methods must be overriden in descendant classes

        public abstract string Category { get; }
        public abstract string ParentCategory { get; }
        public abstract uint Priority { get; }
        public abstract void ExportData(ExportImportJob exportJob, ExportDto exportDto);
        public abstract void ImportData(ExportImportJob importJob, ExportDto exportDto);
    }
}