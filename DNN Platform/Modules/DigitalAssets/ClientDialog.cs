// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Web.UI;

namespace DotNetNuke.Modules.DigitalAssets
{
    public static class ClientDialog
    {
        public static void CloseClientDialog(this Page page, bool refresh)
        {
            var script = "parent.window.dnnModule.digitalAssets.closeDialog(" + (refresh ? "true" : "false") + ");";
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "CloseDialogScript", script, true);
        }
    }
}
