// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Common.Controls
{
    public partial class Message : PortalModuleBase
    {
        private void InitializeComponent()
        {
            ID = "Message";
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InitializeComponent();
        }
    }
}
