﻿#region Copyright

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

namespace DotNetNuke.Framework.JavaScriptLibraries
{
    /// <summary>
    ///     this class contains a number of constants that map to <see cref="JavaScriptLibrary.LibraryName"/>s
    ///     done as a series of constants as enums do not allow hyphens or periods
    /// </summary>
    public static class CommonJs
    {
        /// <summary>jQuery library name</summary>
        public const string jQuery = "jQuery";

        /// <summary>jQuery Migrate library name</summary>
        public const string jQueryMigrate = "jQuery-Migrate";

        /// <summary>jQuery UI library name</summary>
        public const string jQueryUI = "jQuery-UI";

        /// <summary>Knockout library name</summary>
        public const string Knockout = "Knockout";

        /// <summary>Knockout Mapping library name</summary>
        public const string KnockoutMapping = "Knockout.Mapping";

        /// <summary>jQuery Fileupload library name</summary>
        public const string jQueryFileUpload = "jQuery.Fileupload";

        /// <summary>DNN jQuery plugins library name</summary>
        public const string DnnPlugins = "DnnPlugins";

        /// <summary>HoverIntent library name</summary>
        public const string HoverIntent = "HoverIntent";
    }
}