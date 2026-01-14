// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <summary>A form section control.</summary>
    [ParseChildren(true)]
    public class DnnFormSection : WebControl, INamingContainer
    {
        /// <summary>Initializes a new instance of the <see cref="DnnFormSection"/> class.</summary>
        public DnnFormSection()
        {
            this.Items = new List<DnnFormItemBase>();
        }

        /// <summary>Gets or sets a value indicating whether the section is expanded.</summary>
        public bool Expanded { get; set; }

        /// <summary>Gets the items.</summary>
        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormItemBase> Items { get; private set; }

        /// <summary>Gets or sets the resource key.</summary>
        public string ResourceKey { get; set; }
    }
}
