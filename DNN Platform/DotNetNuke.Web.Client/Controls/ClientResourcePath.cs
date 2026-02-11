// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System;
    using System.Web.UI;

    /// <summary>Defines the path to a client resource.</summary>
    public class ClientResourcePath
    {
        /// <summary>An event which is triggered during data binding.</summary>
        public event EventHandler DataBinding;

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the path.</summary>
        public string Path { get; set; }

        /// <summary>Gets the parent.</summary>
        public ClientResourceLoader Parent { get; internal set; }

        /// <summary>Gets the binding container.</summary>
        public Control BindingContainer => this.Parent;

        /// <summary>Triggers the <see cref="DataBinding"/> event.</summary>
        public void DataBind()
        {
            this.OnDataBinding(EventArgs.Empty);
        }

        /// <summary>Triggers the <see cref="DataBinding"/> event.</summary>
        /// <param name="e">The event args.</param>
        protected void OnDataBinding(EventArgs e)
        {
            this.DataBinding?.Invoke(this, e);
        }
    }
}
