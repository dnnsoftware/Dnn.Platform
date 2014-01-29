#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Modules.Admin.Host
{
    public partial class WhatsNew : PortalModuleBase
    {

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            jQuery.RequestDnnPluginsRegistration();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var resourcefile = Server.MapPath(LocalResourceFile + ".ascx.resx");
            if (File.Exists(resourcefile))
            {
                var document = new XPathDocument(resourcefile);
                var navigator = document.CreateNavigator();

                var nodes = navigator.Select("/root/data[starts-with(@name, 'WhatsNew')]/@name");

                var releasenotes = new List<ReleaseInfo>();

                while (nodes.MoveNext())
                {
                    var key = nodes.Current.Value;
                    var version = string.Format(Localization.GetString("notestitle.text", LocalResourceFile), key.Replace("WhatsNew.", string.Empty));
                    releasenotes.Add(new ReleaseInfo(Localization.GetString(key, LocalResourceFile), version));
                }

                releasenotes.Sort(CompareReleaseInfo);

                WhatsNewList.DataSource = releasenotes;
                WhatsNewList.DataBind();

                header.InnerHtml = Localization.GetString("header.text", LocalResourceFile);
                footer.InnerHtml = Localization.GetString("footer.text", LocalResourceFile);
            }
        }

        #endregion

        #region Private Methods

        private static int CompareReleaseInfo(ReleaseInfo notes1, ReleaseInfo notes2)
        {
			//We do this in reverse order so that we have the latest release at the top of the list
            return notes2.Version.CompareTo(notes1.Version);
        }

        #endregion

        #region Nested type: ReleaseInfo

        internal class ReleaseInfo
        {
            /// <summary>
            /// Initializes a new instance of the ReleaseInfo class.
            /// </summary>
            /// <param name="notes"></param>
            /// <param name="version"></param>
            public ReleaseInfo(string notes, string version)
            {
                Notes = notes;
                Version = version;
            }

            public string Notes { get; set; }

            public string Version { get; set; }
        }

        #endregion

    }
}