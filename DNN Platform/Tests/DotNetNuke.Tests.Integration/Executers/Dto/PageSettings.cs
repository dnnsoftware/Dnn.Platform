// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Executers.Dto
{
    using System;
    using System.Collections.Generic;

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

        public bool isSecure { get; set; }
    }
}
