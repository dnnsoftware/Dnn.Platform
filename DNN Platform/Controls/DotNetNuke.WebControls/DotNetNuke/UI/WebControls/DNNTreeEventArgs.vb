' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System

Namespace DotNetNuke.UI.WebControls
    Public Class DNNTreeEventArgs
        Inherits EventArgs
        ' Methods
        Public Sub New(ByVal node As TreeNode)
            Me._node = node
        End Sub


        ' Properties
        Public ReadOnly Property Node As TreeNode
            Get
                Return Me._node
            End Get
        End Property


        ' Fields
        Private _node As TreeNode
    End Class
End Namespace

