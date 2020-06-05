﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Globalization;

namespace DotNetNuke.Services.GeneratedImage
{
    [Bindable(true)]
    public class ImageParameter : IDataBindingsAccessor
    {
        private readonly DataBindingCollection _dataBindings = new DataBindingCollection();

        public string Name { get; set; }

        public string Value { get; set; }

        public event EventHandler DataBinding;

        internal void DataBind()
        {
            if (this.DataBinding != null)
            {
                this.DataBinding(this, EventArgs.Empty);
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Name) && string.IsNullOrEmpty(this.Value))
            {
                return base.ToString();
            }
            return string.Format(CultureInfo.InvariantCulture, "{0} = {1}", this.Name, this.Value);
        }

        public Control BindingContainer { get; internal set; }

        #region IDataBindingsAccessor Members

        DataBindingCollection IDataBindingsAccessor.DataBindings => this._dataBindings;

        bool IDataBindingsAccessor.HasDataBindings => this._dataBindings.Count != 0;

        #endregion
    }
}
