﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow
// ReSharper enable CheckNamespace
{
    /// <summary>
    /// This entity represents a Workflow Log
    /// </summary>
    [Obsolete("Deprecated in Platform 7.4.0")]   
    public class ContentWorkflowLog
    {
        /// <summary>
        /// Workflow log Id
        /// </summary>
        public int WorkflowLogID { get; set; }

        /// <summary>
        /// Workflow associated to the log entry
        /// </summary>
        public int WorkflowID { get; set; }

        /// <summary>
        /// Content Item associated to the log entry
        /// </summary>
        public int ContentItemID { get; set; }
        
        /// <summary>
        /// Action name (usually is a localized representation of the ContentWorkflowLogType)
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Log date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// User Id associated to the log
        /// </summary>
        public int User { get; set; }
    }
}
