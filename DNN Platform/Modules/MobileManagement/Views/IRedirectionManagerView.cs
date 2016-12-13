#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion
using System;
using System.Web.UI.WebControls;
using DotNetNuke.Modules.MobileManagement.ViewModels;
using DotNetNuke.Web.Mvp;

namespace DotNetNuke.Modules.MobileManagement.Views
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRedirectionManagerView : IModuleView<RedirectionManagerViewModel>
    {
        event EventHandler<DataGridCommandEventArgs> RedirectionItemAction;
    }
}
