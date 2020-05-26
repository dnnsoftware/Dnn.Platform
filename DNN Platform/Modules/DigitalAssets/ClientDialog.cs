// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
