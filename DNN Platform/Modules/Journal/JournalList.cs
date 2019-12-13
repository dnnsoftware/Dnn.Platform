﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotNetNuke.Modules.Journal {
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:JournalList runat=server></{0}:JournalList>")]
    public class JournalList : WebControl {
        public int PortalId { get; set; }
        public int ProfileId { get; set; }
        public int SocialGroupId { get; set; }
        public int DisplayMode { get; set; }
        
        protected override void Render(HtmlTextWriter output) {
            output.Write("Hello World");
        }
    }
}
