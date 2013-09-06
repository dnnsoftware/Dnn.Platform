#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using System;
using System.Xml.XPath;

namespace DotNetNuke.Services.Installer.Installers
{
    public class JavaScriptFileInstaller : FileInstaller
    {
 		#region Protected Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("jsfiles")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "jsfiles";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("jsfile")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "jsfile";
            }
        }

		#endregion  
 
        #region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List)
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get
            {
                return "js";
            }
        }


        #endregion

        #region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadCustomManifest method reads the custom manifest items (that subclasses
        /// of FileInstaller may need)
        /// </summary>
        /// <param name="nav">The XPathNavigator representing the node</param>
        /// -----------------------------------------------------------------------------
        protected override void ReadCustomManifest(XPathNavigator nav)
        {
            XPathNavigator libraryNav = nav.SelectSingleNode("libraryFolderName");
            if (libraryNav != null)
            {
                BasePath = String.Format("Resources\\Libraries\\{0}\\{1}", libraryNav.Value, Package.Version);
            }
        }


        #endregion

    }
}
