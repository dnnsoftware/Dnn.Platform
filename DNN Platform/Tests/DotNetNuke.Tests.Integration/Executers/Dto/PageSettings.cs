#region Copyright
//
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Collections.Generic;

namespace DotNetNuke.Tests.Integration.Executers.Dto
{
    public class PageSettings
    {
        public bool ApplyWorkflowToChildren { get; set; }
        public string created { get; set; }
        public bool customUrlEnabled { get; set; }
        public bool hasChild { get; set; }
        public string Hierarchy { get; set; }
        public bool IncludeInMenu { get; set; }
        public bool isCopy { get; set; }
        public bool isWorkflowCompleted { get; set; }
        public bool isWorkflowPropagationAvailable { get; set; }
        public string keywords { get; set; }
        public string localizedName { get; set; }
        public string Name { get; set; }
        public string pageType { get; set; }
        public int tabId { get; set; }
        public string tags { get; set; }
        public string thumbnail { get; set; }
        public string title { get; set; }
        public bool trackLinks { get; set; }
        public int type { get; set; }
        public int workflowId { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public TabPermissions permissions { get; set; }
        public int templateTabId { get; set; }
        public IList<CopyModuleItem> modules { get; set; } 
        public bool isSecure { get; set;  }
    }
}
