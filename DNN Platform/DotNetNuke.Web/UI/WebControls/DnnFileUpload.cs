// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.Common;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A file upload control.</summary>
    [ToolboxData("<{0}:DnnFileUpload runat='server'></{0}:DnnFileUpload>")]
    public class DnnFileUpload : Control, INamingContainer
    {
        private readonly IClientResourceController clientResourceController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;
        private readonly IServicesFramework servicesFramework;
        private readonly Lazy<DnnFileUploadOptions> options;

        /// <summary>Initializes a new instance of the <see cref="DnnFileUpload"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public DnnFileUpload()
            : this(null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnFileUpload"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        public DnnFileUpload(IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, IHostSettings hostSettings, ICryptographyProvider cryptographyProvider, IServicesFramework servicesFramework)
        {
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
            this.options = new Lazy<DnnFileUploadOptions>(() => new DnnFileUploadOptions(hostSettings, this.appStatus, cryptographyProvider));
        }

        /// <summary>Gets the options.</summary>
        public DnnFileUploadOptions Options => this.options.Value;

        /// <summary>Sets the module ID.</summary>
        public int ModuleId
        {
            set
            {
                var moduleIdString = value.ToString(CultureInfo.InvariantCulture);
                this.Options.ModuleId = moduleIdString;
                this.Options.FolderPicker.Services.ModuleId = moduleIdString;
            }
        }

        /// <summary>Sets the parent client ID.</summary>
        public string ParentClientId
        {
            set => this.Options.ParentClientId = value;
        }

        /// <summary>Sets a value indicating whether to show on startup.</summary>
        public bool ShowOnStartup
        {
            set => this.Options.ShowOnStartup = value;
        }

        /// <summary>Gets or sets the skin.</summary>
        public string Skin { get; set; }

        /// <summary>Gets or sets a value indicating whether the host file system is supported.</summary>
        public bool SupportHost { get; set; }

        /// <summary>Gets or sets the width in pixels.</summary>
        public int Width
        {
            get => this.Options.Width;
            set => this.Options.Width = value;
        }

        /// <summary>Gets or sets the height in pixels.</summary>
        public int Height
        {
            get => this.Options.Height;
            set => this.Options.Height = value;
        }

        /// <summary>Gets the current upload control.</summary>
        /// <param name="page">The page.</param>
        /// <returns>The control instance or <see langword="null"/>.</returns>
        public static DnnFileUpload GetCurrent(Page page)
        {
            return page.Items[typeof(DnnFileUpload)] as DnnFileUpload;
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.servicesFramework.RequestAjaxAntiForgerySupport();
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, PortalSettings.Current, CommonJs.jQueryFileUpload);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            RegisterClientScript(this.clientResourceController, this.appStatus, this.eventLogger, PortalSettings.Current, this.Skin);
            this.RegisterStartupScript();
        }

        private static void RegisterClientScript(IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, IPortalSettings portalSettings, string skin)
        {
            DnnDropDownList.RegisterClientScript(clientResourceController, skin);

            clientResourceController.RegisterStylesheet("~/Resources/Shared/Components/FileUpload/dnn.FileUpload.css", FileOrder.Css.ResourceCss);
            if (!string.IsNullOrEmpty(skin))
            {
                clientResourceController.RegisterStylesheet("~/Resources/Shared/Components/FileUpload/dnn.FileUpload." + skin + ".css", FileOrder.Css.ResourceCss);
            }

            JavaScript.RequestRegistration(appStatus, eventLogger, portalSettings, CommonJs.jQueryUI);

            clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.WebResourceUrl.js", FileOrder.Js.DefaultPriority + 2);
            clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.jquery.extensions.js", FileOrder.Js.DefaultPriority + 3);
            clientResourceController.RegisterScript("~/Resources/Shared/Components/FileUpload/dnn.FileUpload.js", FileOrder.Js.DefaultPriority + 4);
        }

        private void RegisterStartupScript()
        {
            this.Options.ClientId = this.ClientID;

            var portalSettings = PortalSettings.Current;

            if (this.Options.FolderPicker.InitialState == null)
            {
                var folder = FolderManager.Instance.GetFolder(portalSettings.PortalId, string.Empty);
                var rootFolder = (this.SupportHost && portalSettings.ActiveTab.IsSuperTab) ? DynamicSharedConstants.HostRootFolder : DynamicSharedConstants.RootFolder;

                this.Options.FolderPicker.InitialState = new DnnDropDownListState
                {
                    SelectedItem = (folder != null) ? new SerializableKeyValuePair<string, string>(folder.FolderID.ToString(CultureInfo.InvariantCulture), rootFolder) : null,
                };
            }

            if (this.Options.Extensions.Count > 0)
            {
                var extensionsText = this.Options.Extensions.Aggregate(string.Empty, (current, extension) => current.Append(extension, ", "));
                this.Options.Resources.InvalidFileExtensions = string.Format(CultureInfo.CurrentCulture, this.Options.Resources.InvalidFileExtensions, extensionsText);
            }

            if (this.Options.MaxFiles > 0)
            {
                this.Options.Resources.TooManyFiles = string.Format(CultureInfo.CurrentCulture, this.Options.Resources.TooManyFiles, this.Options.MaxFiles.ToString(CultureInfo.InvariantCulture));
            }

            if (!this.SupportHost)
            {
                this.Options.FolderPicker.Services.Parameters["portalId"] = portalSettings.PortalId.ToString(CultureInfo.InvariantCulture);
            }

            this.Options.FolderPicker.Services.GetTreeMethod = "ItemListService/GetFolders";
            this.Options.FolderPicker.Services.GetNodeDescendantsMethod = "ItemListService/GetFolderDescendants";
            this.Options.FolderPicker.Services.SearchTreeMethod = "ItemListService/SearchFolders";
            this.Options.FolderPicker.Services.GetTreeWithNodeMethod = "ItemListService/GetTreePathForFolder";
            this.Options.FolderPicker.Services.SortTreeMethod = "ItemListService/SortFolders";
            this.Options.FolderPicker.Services.ServiceRoot = "InternalServices";

            var script = $"dnn.createFileUpload({Json.Serialize(this.Options)});{Environment.NewLine}";

            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), this.ClientID + "DnnFileUpload", script, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "DnnFileUpload", script, true);
            }
        }
    }
}
