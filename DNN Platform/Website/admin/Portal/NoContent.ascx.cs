// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Controls
{
    using System;

    using DotNetNuke.Entities.Modules;

    public partial class NoContent : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
        }
    }
}
