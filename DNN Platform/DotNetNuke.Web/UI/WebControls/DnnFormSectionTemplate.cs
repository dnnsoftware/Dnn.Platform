// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A form section template.</summary>
    internal class DnnFormSectionTemplate : ITemplate
    {
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="DnnFormSectionTemplate"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public DnnFormSectionTemplate()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnFormSectionTemplate"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public DnnFormSectionTemplate(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.Items = [];
        }

        /// <summary>Gets the items.</summary>
        public List<DnnFormItemBase> Items { get; private set; }

        /// <summary>Gets or sets the path to the local resource file.</summary>
        public string LocalResourceFile { get; set; }

        /// <inheritdoc />
        public void InstantiateIn(Control container)
        {
            if (container is WebControl webControl)
            {
                DnnFormEditor.SetUpItems(this.hostSettings, this.Items, webControl, false);
            }
        }
    }
}
