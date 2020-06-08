// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Web.UI;

#endregion

namespace DotNetNuke.Common.Utilities
{
    public partial class KeepAlive : Page
    {
		//This call is required by the Web Form Designer.
        private void InitializeComponent()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

			//CODEGEN: This method call is required by the Web Form Designer
			//Do not modify it using the code editor.
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
    }
}
