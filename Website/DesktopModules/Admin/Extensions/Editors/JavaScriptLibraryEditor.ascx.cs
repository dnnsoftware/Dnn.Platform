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

using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.Installer.Packages;

namespace DotNetNuke.Modules.Admin.Extensions
{
    public partial class JavaScriptLibraryEditor : PackageEditorBase
    {
        private JavaScriptLibrary _javaScriptLibrary;

        protected override string EditorID
        {
            get
            {
                return "JavaScriptLibraryEditor";
            }
        }

        public JavaScriptLibrary JavaScriptLibrary
        {
            get 
            {
                return _javaScriptLibrary ?? (_javaScriptLibrary = JavaScriptLibraryController.Instance.GetLibrary(l => l.PackageID == PackageID));
            }
        }

        public override void Initialize()
        {
            LibraryName.Text = JavaScriptLibrary.LibraryName;
            Version.Text = JavaScriptLibrary.Version.ToString();
            ObjectName.Text = JavaScriptLibrary.ObjectName;
            CDN.Text = JavaScriptLibrary.CDNPath;
            FileName.Text = JavaScriptLibrary.FileName;
            Location.Text = LocalizeString(JavaScriptLibrary.PreferredScriptLocation.ToString());
        }
    }
}