// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteImportExport.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class SiteImportExportController : PersonaBarApiController
    {}
}
