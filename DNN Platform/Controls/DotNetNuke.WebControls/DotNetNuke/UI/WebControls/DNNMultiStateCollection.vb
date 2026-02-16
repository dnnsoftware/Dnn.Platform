' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Reflection

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public Class DNNMultiStateCollection
        Inherits CollectionBase
        ' Methods
        Public Sub New(ByVal ctl As DNNMultiStateBox)
            Me.m_ctl = ctl
        End Sub

        Public Sub Add(ByVal Item As DNNMultiState)
            MyBase.InnerList.Add(Item)
        End Sub

        Public Sub AddAt(ByVal index As Integer, ByVal Item As DNNMultiState)
            MyBase.InnerList.Insert(index, Item)
        End Sub

        Protected Overrides Sub OnInsert(ByVal index As Integer, ByVal value As Object)
            TryCast(value,DNNMultiState).Parent = Me.m_ctl
            Me.m_ctl.NotifyDesigner
        End Sub


        ' Properties
        Public Default Property Item(ByVal index As Integer) As DNNMultiState
            Get
                Return TryCast(MyBase.InnerList(index),DNNMultiState)
            End Get
            Set(ByVal value As DNNMultiState)
                MyBase.InnerList(index) = value
            End Set
        End Property


        ' Fields
        Private m_ctl As DNNMultiStateBox
    End Class
End Namespace

