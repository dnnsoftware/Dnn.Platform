﻿// Licensed to the .NET Foundation under one or more agreements.
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
        [DataMember(Name = "valueField")]
        public string ValueField { get; } = "value";

        [DataMember(Name = "labelField")]
        public string LabelField { get; } = "text";

        [DataMember(Name = "searchField")]
        public string SearchField { get; } = "text";

        [DataMember(Name = "options")]
        public IEnumerable<OptionItem> Options
        {
            get { return this.Items?.Select(i => new OptionItem { Text = i.Text, Value = i.Value, Selected = i.Selected }); }
        }

        [DataMember(Name = "create")]
        public bool Create { get; set; } = false;

        [DataMember(Name = "preload")]
        public string Preload { get; set; }

        [DataMember(Name = "highlight")]
        public bool Highlight { get; set; }

        [DataMember(Name = "allowEmptyOption")]
        public bool AllowEmptyOption { get; set; }

        [DataMember(Name = "plugins")]
        public IList<string> Plugins { get; set; } = new List<string>();

        [DataMember(Name = "checkbox")]
        public bool Checkbox { get; set; }

        [DataMember(Name = "maxOptions")]
        public int MaxOptions { get; set; }

        [DataMember(Name = "maxItems")]
        public int MaxItems { get; set; }

        [IgnoreDataMember]
        public IEnumerable<ListItem> Items { get; set; }

        [DataMember(Name = "localization")]
        public IDictionary<string, string> Localization { get; set; } = new Dictionary<string, string>();

        [DataMember(Name = "render")]
        public RenderOption Render { get; set; }

        [DataMember(Name = "load")]
        public string Load { get; set; }

        [DataMember(Name = "onChange")]
        public string OnChangeEvent { get; set; }
    }
}
