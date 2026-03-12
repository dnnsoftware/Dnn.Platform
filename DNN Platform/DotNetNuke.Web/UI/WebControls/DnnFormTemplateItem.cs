// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A template item control.</summary>
    [ParseChildren(true)]
    public class DnnFormTemplateItem : DnnFormItemBase
    {
        /// <summary>Initializes a new instance of the <see cref="DnnFormTemplateItem"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public DnnFormTemplateItem()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnFormTemplateItem"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public DnnFormTemplateItem(IApplicationStatusInfo appStatus, IEventLogger eventLogger)
            : base(appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(), eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>())
        {
        }

        /// <summary>Gets or sets the item template.</summary>
        [Browsable(false)]
        [DefaultValue(null)]
        [Description("The Item Template.")]
        [TemplateInstance(TemplateInstance.Single)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(DnnFormEmptyTemplate))]
        public ITemplate ItemTemplate { get; set; }

        /// <inheritdoc />
        protected override void CreateControlHierarchy()
        {
            this.CssClass += " dnnFormItem";
            this.CssClass += (this.FormMode == DnnFormMode.Long) ? " dnnFormLong" : " dnnFormShort";

            var template = new DnnFormEmptyTemplate();
            this.ItemTemplate.InstantiateIn(template);
            this.Controls.Add(template);
        }
    }
}
