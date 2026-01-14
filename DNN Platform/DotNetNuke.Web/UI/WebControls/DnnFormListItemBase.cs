// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System.Collections;

    /// <summary>An item in a form.</summary>
    public abstract class DnnFormListItemBase : DnnFormItemBase
    {
        private IEnumerable listSource;

        /// <summary>Gets or sets the default value.</summary>
        public string DefaultValue { get; set; }

        /// <summary>Gets or sets the list.</summary>
        public IEnumerable ListSource
        {
            get
            {
                return this.listSource;
            }

            set
            {
                var changed = !Equals(this.listSource, value);
                if (changed)
                {
                    this.listSource = value;
                    this.BindList();
                }
            }
        }

        /// <summary>Gets or sets the name of the field/property with the text.</summary>
        public string ListTextField { get; set; }

        /// <summary>Gets or sets the name of the field/property with the value.</summary>
        public string ListValueField { get; set; }

        /// <summary>Bind the list.</summary>
        protected virtual void BindList()
        {
        }
    }
}
