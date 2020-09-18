' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System.Collections

Namespace DotNetNuke.UI.Utilities

    Public Class BrowserCollection

        Inherits CollectionBase

        Public Sub Add(ByVal b As Browser)
            Me.InnerList.Add(b)
        End Sub

        Default Public Overridable Property Item(ByVal index As Integer) As Browser
            Get
                Return DirectCast(MyBase.List.Item(index), Browser)
            End Get
            Set(ByVal Value As Browser)
                MyBase.List.Item(index) = Value
            End Set
        End Property

    End Class

End Namespace
