' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Utilities
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.ComponentModel
Imports System.Web.UI
Imports System.Xml

Namespace DotNetNuke.UI.WebControls
    Public Class TreeNode
        Inherits DNNNode
        Implements IStateManager
        ' Methods
        Public Sub New()
        End Sub

        Public Sub New(ByVal strText As String)
            MyBase.New(strText)
        End Sub

        Friend Sub New(ByVal ctlOwner As Control)
            Me.m_objDNNTree = DirectCast(ctlOwner, DnnTree)
        End Sub

        Friend Sub New(ByVal objXmlNode As XmlNode, ByVal ctlOwner As Control)
            MyBase.New(objXmlNode)
            Me.m_objDNNTree = DirectCast(ctlOwner, DnnTree)
        End Sub

        Public Sub Click()
            Me.Selected = True
            Me.DNNTree.OnNodeClick(New DNNTreeNodeClickEventArgs(Me))
        End Sub

        Public Sub Collapse()
            If Me.HasNodes Then
                Me.CustomAttribute("expanded") = "0"
                Me.DNNTree.OnCollapse(New DNNTreeEventArgs(Me))
            End If
        End Sub

        Public Sub Expand()
            If Me.HasNodes Then
                Me.CustomAttribute("expanded") = "1"
                Me.DNNTree.OnExpand(New DNNTreeEventArgs(Me))
            End If
        End Sub

        Public Sub LoadViewState(ByVal state As Object) Implements IStateManager.LoadViewState
        End Sub

        Public Sub MakeNodeVisible()
            If Not Object.ReferenceEquals(Me.Parent, Nothing) Then
                Me.Parent.Expand
                Me.Parent.MakeNodeVisible
            End If
        End Sub

        Public Overridable Sub Render(ByVal writer As HtmlTextWriter)
            Me.NodeWriter.RenderNode(writer, Me)
        End Sub

        Public Function SaveViewState() As Object Implements IStateManager.SaveViewState
            Dim obj2 As Object = Nothing
            Return obj2
        End Function

        Friend Sub SetDNNTree(ByVal objTree As DnnTree)
            Me.m_objDNNTree = objTree
        End Sub

        Public Sub TrackViewState() Implements IStateManager.TrackViewState
        End Sub


        ' Properties
        Public Shadows Property CssClass As String
            Get
                Return MyBase.CSSClass
            End Get
            Set(ByVal Value As String)
                MyBase.CSSClass = Value
            End Set
        End Property

        Public Shadows Property NavigateUrl As String
            Get
                Return MyBase.NavigateURL
            End Get
            Set(ByVal Value As String)
                MyBase.NavigateURL = Value
            End Set
        End Property

        <Browsable(True), PersistenceMode(PersistenceMode.InnerProperty)> _
        Public ReadOnly Property TreeNodes As TreeNodeCollection
            Get
                If Object.ReferenceEquals(Me.m_objNodes, Nothing) Then
                    Me.m_objNodes = New TreeNodeCollection(Me.XmlNode, Me.DNNTree)
                End If
                Return Me.m_objNodes
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property Parent As TreeNode
            Get
                Return Me.ParentNode
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property DNNTree As DnnTree
            Get
                Return Me.m_objDNNTree
            End Get
        End Property

        <Browsable(False), DefaultValue(False)> _
        Public ReadOnly Property IsExpanded As Boolean
            Get
                Dim obj2 As Object
                If Me.DNNTree.IsDownLevel Then
                    obj2 = Conversions.ToBoolean(Me.CustomAttribute("expanded", "0"))
                Else
                    Dim clientVariable As String = ClientAPI.GetClientVariable(Me.m_objDNNTree.Page, (Me.m_objDNNTree.ClientID & "_" & Me.ClientID & ":expanded"))
                    obj2 = If((Strings.Len(clientVariable) <= 0), Conversions.ToBoolean(Me.CustomAttribute("expanded", "0")), Conversions.ToBoolean(clientVariable))
                End If
                Return Conversions.ToBoolean(obj2)
            End Get
        End Property

        <Bindable(True), PersistenceMode(PersistenceMode.Attribute), DefaultValue("")> _
        Public Property CssClassOver As String
            Get
                Return Me.CSSClassHover
            End Get
            Set(ByVal Value As String)
                Me.CSSClassHover = Value
            End Set
        End Property

        <DefaultValue(-1), PersistenceMode(PersistenceMode.Attribute), Bindable(True)> _
        Public Property ImageIndex As Integer
            Get
                Return If((Strings.Len(Me.CustomAttribute("imgIdx")) <= 0), If((Me.DNNTree.ImageList.Count <= 0), -1, 0), Conversions.ToInteger(Me.CustomAttribute("imgIdx")))
            End Get
            Set(ByVal Value As Integer)
                Me.CustomAttribute("imgIdx") = Conversions.ToString(Value)
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

        Public ReadOnly Shadows Property ParentNode As TreeNode
            Get
                Return If(((Me.XmlNode.ParentNode Is Nothing) OrElse (Me.XmlNode.ParentNode.NodeType = XmlNodeType.Document)), Nothing, New TreeNode(Me.XmlNode.ParentNode, Me.m_objDNNTree))
            End Get
        End Property

        Private ReadOnly Property NodeWriter As ITreeNodeWriter
            Get
                Return If(Not Me.m_objDNNTree.IsDownLevel, Nothing, New TreeNodeWriter)
            End Get
        End Property

        Public ReadOnly Property IsTrackingViewState As Boolean Implements IStateManager.IsTrackingViewState
            Get
                Return False
            End Get
        End Property


        ' Fields
        Friend Shared _separator As String = ":"
        Friend Shared ReadOnly _checkboxIDSufix As String = "checkbox"
        Private m_objNodes As TreeNodeCollection
        Private m_objDNNTree As DnnTree
    End Class
End Namespace

