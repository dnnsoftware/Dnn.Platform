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

using Dnn.ExportImport.Components.Models;

namespace Dnn.ExportImport.Components.Interfaces
{
    /// <summary>
    /// Classes derived from this interface should be able to perform
    /// export and import operations of a specific item in the syte
    /// </summary>
    public interface IExportEngine
    {
        /// <summary>
        ///  Indicates the completed percentage progress of export/import operation.
        /// </summary>
        int ProgressPercentage { get; }

        /// <summary>
        ///  Performs export operation of the object
        /// </summary>
        ExportResult Export(ExportData exportData);

        /// <summary>
        ///  Performs import operation of the object
        /// </summary>
        ImportResult Import(ImportData importData);
    }
}