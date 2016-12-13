#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion
#region Usings

using System.Collections.Generic;
using DotNetNuke.Services.Mobile;
using DotNetNuke.Web.Mvp;

#endregion

namespace DotNetNuke.Modules.MobileManagement.ViewModels
{
    public class RedirectionManagerViewModel
    {
        [ViewState]
        public IList<IRedirection> Redirections { get; set; }
        public string AddUrl { get; set; }
        public string ModeType { get; set; }
    }
}