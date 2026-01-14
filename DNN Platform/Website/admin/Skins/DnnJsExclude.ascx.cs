// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    /// <summary>A control which causes JavaScript to no longer be included on the page.</summary>
    public partial class DnnJsExclude : SkinObjectBase
    {
        public string Name { get; set; }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.ctlExclude.Name = this.Name;
        }
    }
}
