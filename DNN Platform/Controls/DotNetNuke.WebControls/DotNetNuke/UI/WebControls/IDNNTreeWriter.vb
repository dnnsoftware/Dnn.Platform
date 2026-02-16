' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    Friend Interface IDNNTreeWriter
        ' Methods
        Function MarshalledProperties() As Hashtable
        Sub RenderTree(ByVal writer As HtmlTextWriter, ByVal tree As DnnTree)
    End Interface
End Namespace

