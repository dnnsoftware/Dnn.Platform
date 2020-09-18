// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow

// ReSharper enable CheckNamespace
{
    using System;

    [Obsolete("Deprecated in Platform 7.4.0.. Scheduled removal in v10.0.0.")]
    public class ContentWorkflowSource
    {
        public int WorkflowId { get; set; }

        public int SourceId { get; set; }

        public string SourceName { get; set; }

        public string SourceType { get; set; }
    }
}
