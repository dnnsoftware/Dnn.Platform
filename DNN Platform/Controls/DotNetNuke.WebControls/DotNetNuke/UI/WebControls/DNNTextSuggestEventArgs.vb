' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System

Namespace DotNetNuke.UI.WebControls
    Public Class DNNTextSuggestEventArgs
        Inherits EventArgs
        ' Methods
        Public Sub New(ByVal nodes As DNNNodeCollection, ByVal [text] As String)
            Me._nodes = nodes
            Me._text = [text]
        End Sub


        ' Properties
        Public ReadOnly Property Nodes As DNNNodeCollection
            Get
                Return Me._nodes
            End Get
        End Property

        Public ReadOnly Property [Text] As String
            Get
                Return Me._text
            End Get
        End Property


        ' Fields
        Private _nodes As DNNNodeCollection
        Private _text As String
    End Class
End Namespace

