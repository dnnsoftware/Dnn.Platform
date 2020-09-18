// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [ParseChildren(true)]
    public class DnnFormTab : WebControl, INamingContainer
    {
        public DnnFormTab()
        {
            this.Sections = new List<DnnFormSection>();
            this.Items = new List<DnnFormItemBase>();
        }

        public bool IncludeExpandAll { get; set; }

        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormItemBase> Items { get; private set; }

        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormSection> Sections { get; private set; }

        public string ResourceKey { get; set; }
        internal string ExpandAllScript { get; set; }
    }
}
