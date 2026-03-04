' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.ComponentModel
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    <PersistChildren(True), ParseChildren(False)> _
    Public Class DNNMultiState
        ' Methods
        Public Sub New()
            Me.m_Key = ""
            Me.m_Text = ""
            Me.m_ImageUrl = ""
            Me.m_DisabledImageUrl = ""
            Me.m_ToolTip = ""
        End Sub

        Public Sub New(ByVal Key As String, ByVal [Text] As String, ByVal ImageUrl As String, ByVal DisabledImageUrl As String, ByVal ToolTip As String)
            Me.m_Key = ""
            Me.m_Text = ""
            Me.m_ImageUrl = ""
            Me.m_DisabledImageUrl = ""
            Me.m_ToolTip = ""
            Me.m_Key = Key
            Me.m_Text = [Text]
            Me.m_ImageUrl = ImageUrl
            Me.m_DisabledImageUrl = DisabledImageUrl
            Me.m_ToolTip = ToolTip
        End Sub

        Friend Sub NotifyDesigner()
            If Not Object.ReferenceEquals(Me.Parent, Nothing) Then
                Me.Parent.NotifyDesigner
            End If
        End Sub


        ' Properties
        <NotifyParentProperty(True)> _
        Public Property Key As String
            Get
                Return Me.m_Key
            End Get
            Set(ByVal Value As String)
                Me.m_Key = Value
                Me.NotifyDesigner
            End Set
        End Property

        <DefaultValue(""), Category("Appearance"), NotifyParentProperty(True)> _
        Public Property [Text] As String
            Get
                Return Me.m_Text
            End Get
            Set(ByVal Value As String)
                Me.m_Text = Value
                Me.NotifyDesigner
            End Set
        End Property

        <DefaultValue(""), NotifyParentProperty(True), Category("Appearance")> _
        Public Property ToolTip As String
            Get
                Return Me.m_ToolTip
            End Get
            Set(ByVal Value As String)
                Me.m_ToolTip = Value
                Me.NotifyDesigner
            End Set
        End Property

        <DefaultValue(""), NotifyParentProperty(True), Category("Appearance")> _
        Public Property ImageUrl As String
            Get
                Return Me.m_ImageUrl
            End Get
            Set(ByVal Value As String)
                Me.m_ImageUrl = Value
                Me.NotifyDesigner
            End Set
        End Property

        <DefaultValue(""), Category("Appearance"), NotifyParentProperty(True)> _
        Public Property DisabledImageUrl As String
            Get
                Return Me.m_DisabledImageUrl
            End Get
            Set(ByVal Value As String)
                Me.m_DisabledImageUrl = Value
                Me.NotifyDesigner
            End Set
        End Property


        ' Fields
        Friend Parent As DNNMultiStateBox
        Private m_Key As String
        Private m_Text As String
        Private m_ImageUrl As String
        Private m_DisabledImageUrl As String
        Private m_ToolTip As String
    End Class
End Namespace

