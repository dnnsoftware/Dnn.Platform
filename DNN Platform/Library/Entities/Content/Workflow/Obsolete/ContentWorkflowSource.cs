// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow
// ReSharper enable CheckNamespace
{
    [Obsolete("Deprecated in Platform 7.4.0.. Scheduled removal in v10.0.0.")]
    public class ContentWorkflowSource
    {
        public int WorkflowId { get; set; }

        public int SourceId { get; set; }

        public string SourceName { get; set; }

        public string SourceType { get; set; }
    }
}
