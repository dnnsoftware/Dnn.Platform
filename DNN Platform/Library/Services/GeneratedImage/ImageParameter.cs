// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.UI;

    [Bindable(true)]
    public class ImageParameter : IDataBindingsAccessor
    {
        private readonly DataBindingCollection _dataBindings = new DataBindingCollection();

        public event EventHandler DataBinding;

        public string Name { get; set; }

        public string Value { get; set; }

        public Control BindingContainer { get; internal set; }

        DataBindingCollection IDataBindingsAccessor.DataBindings => this._dataBindings;

        bool IDataBindingsAccessor.HasDataBindings => this._dataBindings.Count != 0;

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Name) && string.IsNullOrEmpty(this.Value))
            {
                return base.ToString();
            }

            return string.Format(CultureInfo.InvariantCulture, "{0} = {1}", this.Name, this.Value);
        }

        internal void DataBind()
        {
            if (this.DataBinding != null)
            {
                this.DataBinding(this, EventArgs.Empty);
            }
        }
    }
}
