// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Common;

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnImageEditControl : DnnFileEditControl
    {
        public DnnImageEditControl()
        {
            FileFilter = Globals.glbImageFileTypes;
        }
    }
}
