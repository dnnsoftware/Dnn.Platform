﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Web.UI;

    /// <summary>A page which does minimal work but keeps the site's application pool active.</summary>
    public partial class KeepAlive : Page
    {
        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // CODEGEN: This method call is required by the Web Form Designer
            // Do not modify it using the code editor.
            this.InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        // This call is required by the Web Form Designer.
        private void InitializeComponent()
        {
        }
    }
}
