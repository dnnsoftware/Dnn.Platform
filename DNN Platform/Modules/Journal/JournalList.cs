// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [DefaultProperty("Text")]
    [ToolboxData("<{0}:JournalList runat=server></{0}:JournalList>")]
    public class JournalList : WebControl
    {
        public int PortalId { get; set; }

        public int ProfileId { get; set; }

        public int SocialGroupId { get; set; }

        public int DisplayMode { get; set; }

        protected override void Render(HtmlTextWriter output)
        {
            output.Write("Hello World");
        }
    }
}
