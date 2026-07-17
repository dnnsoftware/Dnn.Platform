// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Models
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Modules;

    /// <summary>
    /// Represents the actions and UI state associated with an MVC module.
    /// </summary>
    public class ModuleActionsModel
    {
        // public ModuleInstanceContext ModuleContext { get; internal set; }

        /// <summary>
        /// Gets or sets the module context for which actions are defined.
        /// </summary>
        public ModuleInfo ModuleContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the module supports quick settings.
        /// </summary>
        public bool SupportsQuickSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quick settings should be displayed.
        /// </summary>
        public bool DisplayQuickSettings { get; set; }

        /// <summary>
        /// Gets or sets the model used to render quick settings.
        /// </summary>
        public object QuickSettingsModel { get; set; }

        /// <summary>
        /// Gets or sets the JSON describing custom actions for the module.
        /// </summary>
        public string CustomActionsJSON { get; set; }

        /// <summary>
        /// Gets or sets the JSON describing administrative actions for the module.
        /// </summary>
        public string AdminActionsJSON { get; set; }

        /// <summary>
        /// Gets or sets the serialized list of panes that can host the module.
        /// </summary>
        public string Panes { get; set; }

        /// <summary>
        /// Gets or sets the text for the custom actions group.
        /// </summary>
        public string CustomText { get; set; }

        /// <summary>
        /// Gets or sets the text for the admin actions group.
        /// </summary>
        public string AdminText { get; set; }

        /// <summary>
        /// Gets or sets the text used for move operations.
        /// </summary>
        public string MoveText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the module supports move operations.
        /// </summary>
        public bool SupportsMove { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the module is shared across tabs.
        /// </summary>
        public bool IsShared { get; set; }

        /// <summary>
        /// Gets or sets the module title shown in the actions UI.
        /// </summary>
        public string ModuleTitle { get; set; }

        /// <summary>
        /// Gets or sets client-side scripts associated with actions.
        /// </summary>
        public Dictionary<string, string> ActionScripts { get; set; }
    }
}
