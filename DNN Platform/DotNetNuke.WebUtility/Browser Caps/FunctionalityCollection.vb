' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections

Namespace DotNetNuke.UI.Utilities

    Public Class FunctionalityCollection

        Inherits CollectionBase

        Public Sub Add(ByVal f As FunctionalityInfo)
            Me.InnerList.Add(f)
        End Sub

        Default Public Overridable Property Item(ByVal index As Integer) As FunctionalityInfo
            Get
                Return DirectCast(MyBase.List.Item(index), FunctionalityInfo)
            End Get
            Set(ByVal Value As FunctionalityInfo)
                MyBase.List.Item(index) = Value
            End Set
        End Property

        Default Public Overridable Property Item(ByVal functionality As ClientAPI.ClientFunctionality) As FunctionalityInfo
            Get
                Dim _fInfo As FunctionalityInfo = Nothing
                For Each fInfo As FunctionalityInfo In List
                    If fInfo.Functionality = functionality Then
                        _fInfo = fInfo
                        Exit For
                    End If
                Next
                Return _fInfo
            End Get
            Set(ByVal value As FunctionalityInfo)
                Dim bFound As Boolean = False
                For Each fInfo As FunctionalityInfo In List
                    If fInfo.Functionality = functionality Then
                        fInfo = value
                        bFound = True
                        Exit For
                    End If
                Next
                If Not bFound Then
                    Throw New KeyNotFoundException("Item Not Found")
                End If
            End Set
        End Property

    End Class

End Namespace
