// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <summary>A form tab control.</summary>
    [ParseChildren(true)]
    public class DnnFormTab : WebControl, INamingContainer
    {
        /// <summary>Initializes a new instance of the <see cref="DnnFormTab"/> class.</summary>
        public DnnFormTab()
        {
            this.Sections = new List<DnnFormSection>();
            this.Items = new List<DnnFormItemBase>();
        }

        /// <summary>Gets or sets a value indicating whether to include expand all.</summary>
        public bool IncludeExpandAll { get; set; }

        /// <summary>Gets the list of items.</summary>
        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormItemBase> Items { get; private set; }

        /// <summary>Gets the sections.</summary>
        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormSection> Sections { get; private set; }

        /// <summary>Gets or sets the resource key.</summary>
        public string ResourceKey { get; set; }

        /// <summary>Gets or sets the expand all script.</summary>
        internal string ExpandAllScript { get; set; }
    }
}
