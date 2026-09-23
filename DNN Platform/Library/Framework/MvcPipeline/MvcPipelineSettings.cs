// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework.MvcPipeline
{
    using DotNetNuke.Abstractions.Framework;
    using DotNetNuke.Entities.Modules.Settings;

    internal class MvcPipelineSettings
    {
        [PortalSetting]
        public PagePipeline.PortalRenderingPipeline DefaultPagePipeline { get; set; }
    }
}
