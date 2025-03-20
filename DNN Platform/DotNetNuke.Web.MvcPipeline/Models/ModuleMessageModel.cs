// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Models
{
    using static DotNetNuke.UI.Skins.Controls.ModuleMessage;

    public class ModuleMessageModel
    {
        public string Text { get; set; }

        public string Heading { get; set; }

        public ModuleMessageType IconType { get; set; }

        public string IconImage { get; set; }
    }
}
