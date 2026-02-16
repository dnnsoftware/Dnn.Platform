' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System

Namespace DotNetNuke.UI.WebControls
    Public Class DNNMenuNodeClickEventArgs
        Inherits EventArgs
        ' Methods
        Public Sub New(ByVal Node As MenuNode)
            Me._node = Node
        End Sub


        ' Properties
        Public ReadOnly Property Node As MenuNode
            Get
                Return Me._node
            End Get
        End Property


        ' Fields
        Private _node As MenuNode
    End Class
End Namespace

