#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using Dnn.PersonaBar.Common;
using Dnn.PersonaBar.Common.Attributes;
using DotNetNuke.Instrumentation;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.SearchSettings.Services
{
    [DnnAuthorize(StaticRoles = "Administrators")]
    [ServiceScope(Scope = ServiceScope.Admin)]
    public class SearchSettingsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchSettingsController));
    }
}
