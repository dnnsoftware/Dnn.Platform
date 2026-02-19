' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.ComponentModel
Imports System.Web.UI
Imports System.Xml

Namespace DotNetNuke.UI.WebControls
    Public Class MenuNode
        Inherits DNNNode
        ' Methods
        Public Sub New()
        End Sub

        Public Sub New(ByVal strText As String)
            MyBase.New(strText)
        End Sub

        Friend Sub New(ByVal ctlOwner As Control)
            Me.m_DNNMenu = DirectCast(ctlOwner, DNNMenu)
        End Sub

        Friend Sub New(ByVal objXmlNode As XmlNode, ByVal ctlOwner As Control)
            MyBase.New(objXmlNode)
            Me.m_DNNMenu = DirectCast(ctlOwner, DNNMenu)
        End Sub

        Public Sub Click()
            Me.Selected = Not Me.Selected
            If Me.DNNMenu.IsDownLevel Then
                Dim enumerator As IEnumerator = Nothing
                Try 
                    enumerator = Me.DNNMenu.SelectedMenuNodes.GetEnumerator
                    Do While True
                        If Not enumerator.MoveNext Then
                            Exit Do
                        End If
                        Dim current As MenuNode = DirectCast(enumerator.Current, MenuNode)
                        If (If(((current.Level < Me.Level) OrElse (current.ID = Me.ID)), 0, 1) <> 0) Then
                            current.Selected = False
                        End If
                    Loop
                Finally
                    If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                        TryCast(enumerator,IDisposable).Dispose
                    End If
                End Try
            End If
            Me.DNNMenu.OnNodeClick(New DNNMenuNodeClickEventArgs(Me))
        End Sub

        Public Overridable Sub Render(ByVal writer As HtmlTextWriter)
            Me.NodeWriter.RenderNode(writer, Me)
        End Sub

        Friend Sub SetDNNMenu(ByVal objMenu As DNNMenu)
            Me.m_DNNMenu = objMenu
        End Sub


        ' Properties
        <PersistenceMode(PersistenceMode.InnerProperty), Browsable(True)> _
        Public ReadOnly Property MenuNodes As MenuNodeCollection
            Get
                If Object.ReferenceEquals(Me.m_objNodes, Nothing) Then
                    Me.m_objNodes = New MenuNodeCollection(Me.XmlNode, Me.DNNMenu)
                End If
                Return Me.m_objNodes
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property Parent As MenuNode
            Get
                Return Me.ParentNode
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property DNNMenu As DNNMenu
            Get
                Return Me.m_DNNMenu
            End Get
        End Property

        <DefaultValue(""), PersistenceMode(PersistenceMode.Attribute), Bindable(True)> _
        Public Property CssClassOver As String
            Get
                Return Me.CSSClassHover
            End Get
            Set(ByVal Value As String)
                Me.CSSClassHover = Value
            End Set
        End Property

        <PersistenceMode(PersistenceMode.Attribute), DefaultValue(-1), Bindable(True)> _
        Public Property ImageIndex As Integer
            Get
                Return If((Strings.Len(Me.CustomAttribute("iIdx")) <= 0), -1, Conversions.ToInteger(Me.CustomAttribute("iIdx")))
            End Get
            Set(ByVal Value As Integer)
                Me.CustomAttribute("iIdx") = Conversions.ToString(Value)
            End Set
        End Property

        <Bindable(True), DefaultValue(-1), PersistenceMode(PersistenceMode.Attribute)> _
        Public Property UrlIndex As Integer
            Get
                Return If((Strings.Len(Me.CustomAttribute("uIdx")) <= 0), -1, Conversions.ToInteger(Me.CustomAttribute("uIdx")))
            End Get
            Set(ByVal Value As Integer)
                Me.CustomAttribute("uIdx") = Conversions.ToString(Value)
            End Set
        End Property

        Public Property LeftHTML As String
            Get
                Return Me.CustomAttribute("lhtml", "")
            End Get
            Set(ByVal Value As String)
                Me.CustomAttribute("lhtml") = Value
            End Set
        End Property

        Public Property RightHTML As String
            Get
                Return Me.CustomAttribute("rhtml", "")
            End Get
            Set(ByVal Value As String)
                Me.CustomAttribute("rhtml") = Value
            End Set
        End Property

        Public ReadOnly Shadows Property ParentNode As MenuNode
            Get
                Return If(((Me.XmlNode.ParentNode Is Nothing) OrElse (Me.XmlNode.ParentNode.NodeType = XmlNodeType.Document)), Nothing, New MenuNode(Me.XmlNode.ParentNode, Me.m_DNNMenu))
            End Get
        End Property

        Private ReadOnly Property NodeWriter As IMenuNodeWriter
            Get
                Dim writer As IMenuNodeWriter
                Select Case Me.m_DNNMenu.RenderMode
                    Case DNNMenu.MenuRenderMode.UnorderedList
                        writer = New MenuNodeListWriter(Me.m_DNNMenu.Orientation)
                        Exit Select
                    Case DNNMenu.MenuRenderMode.DownLevel
                        writer = New MenuNodeWriter(Me.m_DNNMenu.IsCrawler, Me.m_DNNMenu.Orientation)
                        Exit Select
                    Case Else
                        writer = New MenuNodeListWriter(Me.m_DNNMenu.Orientation)
                        Exit Select
                End Select
                Return writer
            End Get
        End Property


        ' Fields
        Private m_objNodes As MenuNodeCollection
        Private m_DNNMenu As DNNMenu
    End Class
End Namespace

