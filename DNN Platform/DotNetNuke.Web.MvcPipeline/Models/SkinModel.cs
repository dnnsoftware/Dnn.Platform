// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Models
{
    using System.Collections.Generic;

    using DotNetNuke.Common;

    /// <summary>
    /// Represents the skin definition and resources used to render a page.
    /// </summary>
    public class SkinModel
    {
        private Dictionary<string, PaneModel> panes;

        /// <summary>
        /// Gets the skin source path.
        /// </summary>
        public string SkinSrc { get; internal set; }

        /// <summary>
        /// Gets the directory path of the skin.
        /// </summary>
        public string SkinPath { get; internal set; }

        /// <summary>
        /// Gets the folder path to the Razor view file corresponding to the skin.
        /// </summary>
        public string RazorPath { get; internal set; }

        /// <summary>
        /// Gets the file path to the Razor view file corresponding to the skin.
        /// </summary>
        public string RazorFile { get; internal set; }

        /// <summary>
        /// Gets the collection of panes defined by the skin.
        /// </summary>
        public Dictionary<string, PaneModel> Panes
        {
            get
            {
                return this.panes ?? (this.panes = new Dictionary<string, PaneModel>());
            }
        }

        /// <summary>
        /// Gets or sets the Razor view used for the control panel.
        /// </summary>
        public string ControlPanelRazor { get; set; }

        /// <summary>
        /// Gets the CSS class applied to panes rendered by the skin.
        /// </summary>
        public string PaneCssClass
        {
            get
            {
                /*
                if (Globals.IsEditMode())
                {
                    return "dnnSortable";
                }
                */
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the CSS class applied to the page body when the skin is rendered.
        /// </summary>
        public string BodyCssClass
        {
            get
            {
                if (Globals.IsEditMode())
                {
                    return "dnnEditState";
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets an error message associated with loading the skin.
        /// </summary>
        public string SkinError { get; set; }

        /// <summary>
        /// Gets or sets the list of stylesheets registered by the skin.
        /// </summary>
        public List<RegisteredStylesheet> RegisteredStylesheets { get; set; } = new List<RegisteredStylesheet>();

        /// <summary>
        /// Gets or sets the list of scripts registered by the skin.
        /// </summary>
        public List<RegisteredScript> RegisteredScripts { get; set; } = new List<RegisteredScript>();
    }
}
