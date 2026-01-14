// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web.UI.WebControls;

    using DotNetNuke.Web.UI.WebControls.Extensions;

    /// <summary>This control is only for internal use, please don't reference it in any other place as it may be removed in the future.</summary>
    [DataContract]
    public class DnnComboBoxOption
    {
        /// <summary>Gets the value field name.</summary>
        [DataMember(Name = "valueField")]
        public string ValueField { get; } = "value";

        /// <summary>Gets the label field name.</summary>
        [DataMember(Name = "labelField")]
        public string LabelField { get; } = "text";

        /// <summary>Gets the search field name.</summary>
        [DataMember(Name = "searchField")]
        public string SearchField { get; } = "text";

        /// <summary>Gets the options.</summary>
        [DataMember(Name = "options")]
        public IEnumerable<OptionItem> Options
        {
            get { return this.Items?.Select(i => new OptionItem { Text = i.Text, Value = i.Value, Selected = i.Selected }); }
        }

        /// <summary>Gets or sets a value indicating whether to create.</summary>
        [DataMember(Name = "create")]
        public bool Create { get; set; }

        /// <summary>Gets or sets the preload.</summary>
        [DataMember(Name = "preload")]
        public string Preload { get; set; }

        /// <summary>Gets or sets a value indicating whether to highlight.</summary>
        [DataMember(Name = "highlight")]
        public bool Highlight { get; set; }

        /// <summary>Gets or sets a value indicating whether to allow selecting an empty option.</summary>
        [DataMember(Name = "allowEmptyOption")]
        public bool AllowEmptyOption { get; set; }

        /// <summary>Gets or sets the plugins.</summary>
        [DataMember(Name = "plugins")]
        public IList<string> Plugins { get; set; } = new List<string>();

        /// <summary>Gets or sets a value indicating whether to use a checkbox.</summary>
        [DataMember(Name = "checkbox")]
        public bool Checkbox { get; set; }

        /// <summary>Gets or sets the maximum number of options.</summary>
        [DataMember(Name = "maxOptions")]
        public int MaxOptions { get; set; }

        /// <summary>Gets or sets the maximum number of items.</summary>
        [DataMember(Name = "maxItems")]
        public int MaxItems { get; set; }

        /// <summary>Gets or sets the items.</summary>
        [IgnoreDataMember]
        public IEnumerable<ListItem> Items { get; set; }

        /// <summary>Gets or sets the localization dictionary.</summary>
        [DataMember(Name = "localization")]
        public IDictionary<string, string> Localization { get; set; } = new Dictionary<string, string>();

        /// <summary>Gets or sets the render options.</summary>
        [DataMember(Name = "render")]
        public RenderOption Render { get; set; }

        /// <summary>Gets or sets the load script.</summary>
        [DataMember(Name = "load")]
        public string Load { get; set; }

        /// <summary>Gets or sets the change event.</summary>
        [DataMember(Name = "onChange")]
        public string OnChangeEvent { get; set; }
    }
}
