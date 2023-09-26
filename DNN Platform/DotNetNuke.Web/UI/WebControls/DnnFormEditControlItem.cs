// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Framework;
    using DotNetNuke.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    public class DnnFormEditControlItem : DnnFormItemBase
    {
        private readonly IServiceProvider serviceProvider;
        private EditControl control;

        /// <summary>Initializes a new instance of the <see cref="DnnFormEditControlItem"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public DnnFormEditControlItem()
            : this(Globals.GetCurrentServiceProvider())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnFormEditControlItem"/> class.</summary>
        /// <param name="serviceProvider">The DI container scope.</param>
        public DnnFormEditControlItem(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public string ControlType { get; set; }

        /// <inheritdoc/>
        protected override WebControl CreateControlInternal(Control container)
        {
            this.control = ActivatorUtilities.CreateInstance(this.serviceProvider, Reflection.CreateType(this.ControlType)) as EditControl;
            if (this.control != null)
            {
                this.control.ID = this.ID + "_Control";
                this.control.Name = this.ID;
                this.control.EditMode = PropertyEditorMode.Edit;
                this.control.Required = false;
                this.control.Value = this.Value;
                this.control.OldValue = this.Value;
                this.control.ValueChanged += this.ValueChanged;
                this.control.DataField = this.DataField;

                this.control.CssClass = "dnnFormInput";

                container.Controls.Add(this.control);
            }

            return this.control;
        }

        private void ValueChanged(object sender, PropertyEditorEventArgs e)
        {
            this.UpdateDataSource(this.Value, e.Value, this.DataField);
        }
    }
}
