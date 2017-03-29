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

namespace Dnn.ExportImport.Components.Common
{
    public enum JobType
    {
        Export = 0, // never change these numbers
        Import
    }

    public enum JobStatus
    {
        Submitted = 0, // never change these numbers
        InProgress,
        Successful,
        Failed,
        Cancelled
    }

    /// <summary>
    /// Mode for export job.
    /// </summary>
    public enum ExportMode
    {
        Complete = 0,
        Differential = 1
    }

    /// <summary>
    /// Specifies what to do when there is a collision during the import process.
    /// </summary>
    public enum CollisionResolution
    {
        /// <summary>
        /// Ignore the imported item and continue.
        /// </summary>
        Ignore,

        /// <summary>
        /// Overwrites the existing item upon importing.
        /// </summary>
        Overwrite,
    }
}