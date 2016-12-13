#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion
using System;
using DotNetNuke.Modules.MobileManagement.Views;
using DotNetNuke.Web.Mvp;

namespace DotNetNuke.Modules.MobileManagement.Presenters
{
    /// <summary>
    /// 
    /// </summary>
    public class SimpleSettingsPresenter : ModulePresenter<ISimpleSettingsView>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        public SimpleSettingsPresenter(ISimpleSettingsView view) : base(view)
        {
            Initialize();
        }

        private void Initialize()
        {
        }

        protected override void OnLoad()
        {
            
        }

    }
}