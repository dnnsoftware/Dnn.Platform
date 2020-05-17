﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Application;
using DotNetNuke.Entities.Host;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class DotNetNukeControl : SkinObjectBase
    {
        public string CssClass { get; set; }

        private void InitializeComponent()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!String.IsNullOrEmpty(CssClass))
            {
                hypDotNetNuke.CssClass = CssClass;
            }
			
            //get Product Name and Legal Copyright from constants (Medium Trust)
            hypDotNetNuke.Text = DotNetNukeContext.Current.Application.LegalCopyright;
            hypDotNetNuke.NavigateUrl = DotNetNukeContext.Current.Application.Url;
            
			//show copyright credits?
			Visible = Host.DisplayCopyright;
        }
    }
}
