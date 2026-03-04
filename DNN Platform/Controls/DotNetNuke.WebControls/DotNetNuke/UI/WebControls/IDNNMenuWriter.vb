' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    Friend Interface IDNNMenuWriter
        ' Methods
        Sub RenderMenu(ByVal writer As HtmlTextWriter, ByVal menu As DNNMenu)
    End Interface
End Namespace

