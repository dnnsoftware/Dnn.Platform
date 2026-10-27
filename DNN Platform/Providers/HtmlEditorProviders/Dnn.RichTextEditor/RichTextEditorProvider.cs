// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider
{
    using System.Collections;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using Dnn.RichTextEditorProvider.Web;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Modules.HTMLEditorProvider;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="HtmlEditorProvider"/> backed by the Jodit editor.</summary>
    public class RichTextEditorProvider : HtmlEditorProvider
    {
        private EditorControl htmlEditorControl;
        private string rootImageDirectory = string.Empty;

        /// <inheritdoc/>
        public override ArrayList AdditionalToolbars { get; set; } = new ArrayList();

        /// <inheritdoc/>
        public override string ControlID { get; set; }

        /// <inheritdoc/>
        public override Unit Height
        {
            get => this.htmlEditorControl.Height;
            set => this.htmlEditorControl.Height = value;
        }

        /// <inheritdoc/>
        public override Control HtmlEditorControl => this.htmlEditorControl;

        /// <inheritdoc/>
        public override string RootImageDirectory
        {
            get
            {
                if (!string.IsNullOrEmpty(this.rootImageDirectory))
                {
                    return this.rootImageDirectory;
                }

                // Remove the Application Path from the Home Directory
                return Globals.ApplicationPath != string.Empty
                    ? this.PortalSettings.HomeDirectory.Replace(Globals.ApplicationPath, string.Empty)
                    : this.PortalSettings.HomeDirectory;
            }

            set => this.rootImageDirectory = value;
        }

        /// <inheritdoc/>
        public override string Text
        {
            get => this.htmlEditorControl.Value;
            set => this.htmlEditorControl.Value = value;
        }

        /// <inheritdoc/>
        public override Unit Width
        {
            get => this.htmlEditorControl.Width;
            set => this.htmlEditorControl.Width = value;
        }

        /// <inheritdoc/>
        public override void AddToolbar()
        {
            // Must exist because it exists at the base class
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            this.htmlEditorControl = ActivatorUtilities.CreateInstance<EditorControl>(HttpContextSource.Current.GetScope().ServiceProvider);
            this.htmlEditorControl.ID = this.ControlID;
        }
    }
}
