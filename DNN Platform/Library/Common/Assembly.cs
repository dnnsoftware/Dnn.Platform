#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using Microsoft.VisualBasic.CompilerServices;

#endregion

namespace DotNetNuke.Common
{
    [Obsolete("Deprecated in DNN 5.1. Use DotNetNukeContext.Current.Application properties.")]
    [StandardModule]
    public sealed class Assembly
    {
        #region Public Constants

        [Obsolete("Deprecated in DNN 5.1. Use DotNetNukeContext.Current.Application properties.")]
        public const string glbAppType = "Framework";

        [Obsolete("Deprecated in DNN 5.1. Use DotNetNukeContext.Current.Application properties.")]
        public const string glbAppVersion = "05.01.00";

        [Obsolete("Deprecated in DNN 5.1. Use DotNetNukeContext.Current.Application properties.")]
        public const string glbAppName = "DNNCORP.PE";

        [Obsolete("Deprecated in DNN 5.1. Use DotNetNukeContext.Current.Application properties.")]
        public const string glbAppTitle = "DotNetNuke";

        [Obsolete("Deprecated in DNN 5.1. Use DotNetNukeContext.Current.Application properties.")]
        public const string glbAppDescription = "DotNetNuke Professional Edition";

        [Obsolete("Deprecated in DNN 5.1. Use DotNetNukeContext.Current.Application properties.")]
        public const string glbAppCompany = "DotNetNuke Corporation";

        [Obsolete("Deprecated in DNN 5.1. Use DotNetNukeContext.Current.Application properties.")]
        public const string glbAppUrl = "http://www.dotnetnuke.com";

        [Obsolete("Deprecated in DNN 5.1. Use DotNetNukeContext.Current.Application properties.")]
        public const string glbUpgradeUrl = "http://update.dotnetnuke.com";

        [Obsolete("Deprecated in DNN 5.1. Use DotNetNukeContext.Current.Application properties.")]
        public const string glbLegalCopyright = "DotNetNuke® is copyright 2002-YYYY by DotNetNuke Corporation";

        [Obsolete("Deprecated in DNN 5.1. Use DotNetNukeContext.Current.Application properties.")]
        public const string glbTrademark = "DotNetNuke,DNN";

        [Obsolete("Deprecated in DNN 5.1. Use DotNetNukeContext.Current.Application properties.")]
        public const string glbHelpUrl = "http://www.dotnetnuke.com/default.aspx?tabid=787";

        #endregion

        [Obsolete("Deprecated in DNN 5.1. Use DotNetNukeContext.Current.Application properties.")]
        public static Version ApplicationVersion
        {
            get
            {
                return new Version("05.01.00");
            }
        }
    }
}