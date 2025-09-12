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
        public event EventHandler DataBinding;

        public string Name { get; set; }

        public string Path { get; set; }

        public ClientResourceLoader Parent { get; internal set; }

        public Control BindingContainer
        {
            get
            {
                return this.Parent;
            }
        }

        public void DataBind()
        {
            this.OnDataBinding(new EventArgs());
        }

        protected void OnDataBinding(EventArgs e)
        {
            if (this.DataBinding != null)
            {
                this.DataBinding(this, e);
            }
        }
    }
}
