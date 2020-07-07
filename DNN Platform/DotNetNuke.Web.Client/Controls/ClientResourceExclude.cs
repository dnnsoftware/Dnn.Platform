// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.Controls
{
    using System;
    using System.Web.UI;

    using ClientDependency.Core;
    using ClientDependency.Core.Controls;

    public abstract class ClientResourceExclude : Control
    {
        public string Name { get; set; }

        public ClientDependencyType DependencyType { get; internal set; }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            var loader = this.Page.FindControl("ClientResourceIncludes");
            this.Name = this.Name.ToLowerInvariant();

            if (loader != null)
            {
                ClientDependencyInclude ctlToRemove = null;
                if (!string.IsNullOrEmpty(this.Name))
                {
                    foreach (ClientDependencyInclude ctl in loader.Controls)
                    {
                        if (ctl.Name.ToLowerInvariant() == this.Name && ctl.DependencyType == this.DependencyType)
                        {
                            ctlToRemove = ctl;
                            break;
                        }
                    }
                }

                if (ctlToRemove != null)
                {
                    loader.Controls.Remove(ctlToRemove);
                }
            }
        }
    }
}
