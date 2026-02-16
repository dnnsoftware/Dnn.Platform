' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System

Namespace DotNetNuke.UI.WebControls
    Public Class DNNLabelEditEventArgs
        Inherits EventArgs
        ' Methods
        Public Sub New(ByVal [text] As String)
            Me._text = [text]
            Me._returnedText = [text]
        End Sub


        ' Properties
        Public ReadOnly Property [Text] As String
            Get
                Return Me._text
            End Get
        End Property

        Public Property ReturnedText As String
            Get
                Return Me._returnedText
            End Get
            Set(ByVal value As String)
                Me._returnedText = value
            End Set
        End Property


        ' Fields
        Private _text As String
        Private _returnedText As String
    End Class
End Namespace

