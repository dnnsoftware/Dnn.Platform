// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Models
{
    using System.Collections.Generic;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Web.MvcPipeline.Controllers;

    public class SkinModel
    {
        private Dictionary<string, PaneModel> panes;

        public string SkinSrc { get; set; }

        public Dictionary<string, PaneModel> Panes
        {
            get
            {
                return this.panes ?? (this.panes = new Dictionary<string, PaneModel>());
            }
        }

        public string RazorFile
        {
            get
            {
                return Path.GetDirectoryName(this.SkinSrc).Replace("\\", "/") + "/Views/" + Path.GetFileName(this.SkinSrc).Replace(".ascx", ".cshtml");
            }
        }

        public string SkinPath
        {
            get
            {
                return Path.GetDirectoryName(this.SkinSrc).Replace("\\", "/") + "/";
            }
        }

        public string ControlPanelRazor { get; set; }

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

        public string SkinError { get; set; }

        public List<RegisteredStylesheet> RegisteredStylesheets { get; set; } = new List<RegisteredStylesheet>();

        public List<RegisteredScript> RegisteredScripts { get; set; } = new List<RegisteredScript>();
    }
}
