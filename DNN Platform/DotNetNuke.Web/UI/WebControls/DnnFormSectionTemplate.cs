// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System.Collections.Generic;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    internal class DnnFormSectionTemplate : ITemplate
    {
        public DnnFormSectionTemplate()
        {
            this.Items = new List<DnnFormItemBase>();
        }

        public List<DnnFormItemBase> Items { get; private set; }

        public string LocalResourceFile { get; set; }

        public void InstantiateIn(Control container)
        {
            var webControl = container as WebControl;
            if (webControl != null)
            {
                DnnFormEditor.SetUpItems(this.Items, webControl, this.LocalResourceFile, false);
            }
        }
    }
}
