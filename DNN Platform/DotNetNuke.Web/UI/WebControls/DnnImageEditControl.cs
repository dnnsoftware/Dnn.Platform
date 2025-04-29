// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls;

using DotNetNuke.Common;

public class DnnImageEditControl : DnnFileEditControl
{
    /// <summary>Initializes a new instance of the <see cref="DnnImageEditControl"/> class.</summary>
    public DnnImageEditControl()
    {
        this.FileFilter = Globals.glbImageFileTypes;
    }
}
