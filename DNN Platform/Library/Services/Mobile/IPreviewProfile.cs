// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mobile
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Modules;

    public interface IPreviewProfile
    {
        int Id { get; set; }

        int PortalId { get; set; }

        string Name { get; set; }

        int Width { get; set; }

        string UserAgent { get; set; }

        int Height { get; set; }

        int SortOrder { get; set; }
    }
}
