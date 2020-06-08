// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings
using System;
using ClientDependency.Core.Controls;
using ClientDependency.Core;
#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    public partial class DnnCssExclude : SkinObjectBase
    {
        public string Name { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ctlExclude.Name = Name;
        }
    }
}
