' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Utilities
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Text
Imports System.Xml

Namespace DotNetNuke.UI.WebControls
    Public Class DNNNode

        ' Methods
        Public Sub New()
            Me.New(New XmlDocument().CreateNode(XmlNodeType.Element, "n", ""))
        End Sub

        Public Sub New(ByVal strText As String)
            Me.New()
            Me.Text = strText
        End Sub

        Public Sub New(ByVal objXmlNode As XmlNode)
            Me.m_objXMLNode = objXmlNode
            Me.m_objXMLDoc = objXmlNode.OwnerDocument
        End Sub

        Public Sub New(ByVal NodeText As String, ByVal navigateUrl As String)
            If (NodeText Is Nothing) Then
                Throw New ArgumentNullException(NameOf(NodeText))
            End If
            If (navigateUrl Is Nothing) Then
                Throw New ArgumentNullException(NameOf(navigateUrl))
            End If
            Me.Text = NodeText
            navigateUrl = navigateUrl
        End Sub

        Friend Sub AssociateXmlNode(ByVal objXmlNode As XmlNode)
            Me.m_objXMLNode = objXmlNode
            Me.m_objXMLDoc = objXmlNode.OwnerDocument
        End Sub

        Public Function Clone() As DNNNode
            Return Me.Clone(True)
        End Function

        Public Function Clone(ByVal blnDeep As Boolean) As DNNNode
            Return New DNNNode(Me.XmlNode.CloneNode(blnDeep))
        End Function

        Public Function ToJSON() As String
            Return Me.ToJSON(True)
        End Function

        Public Function ToJSON(ByVal blnDeep As Boolean) As String
            Dim enumerator As IEnumerator = Nothing
            Dim builder As New StringBuilder
            Try 
                enumerator = Me.XmlNode.Attributes.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As XmlAttribute = DirectCast(enumerator.Current, XmlAttribute)
                    If (builder.Length = 0) Then
                        builder.Append("{"c)
                    Else
                        builder.Append(","c)
                    End If
                    If blnDeep Then
                        builder.Append(current.Name)
                    Else
                        Dim name As String = current.Name
                        If (name = "txt") Then
                            builder.Append("text")
                        ElseIf (name = "tar") Then
                            builder.Append("target")
                        ElseIf (name = "tTip") Then
                            builder.Append("toolTip")
                        Else
                            builder.Append(current.Name)
                        End If
                    End If
                    builder.Append(":"c)
                    builder.Append(("""" & ClientAPI.GetSafeJSString(current.Value) & """"))
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            If blnDeep Then
                builder.Append(","c)
                builder.Append(Me.DNNNodes.ToJSON)
            End If
            builder.Append("}"c)
            Return builder.ToString
        End Function

        Public Function ToXML() As String
            Return Me.XmlNode.OuterXml
        End Function


        ' Properties
        Public ReadOnly Property IsInHierarchy As Boolean
            Get
                Return Not Object.ReferenceEquals(Me.XmlNode.ParentNode, Nothing)
            End Get
        End Property

        Friend ReadOnly Property XMLDoc As XmlDocument
            Get
                Return Me.m_objXMLDoc
            End Get
        End Property

        Friend ReadOnly Property XmlNode As XmlNode
            Get
                Return Me.m_objXMLNode
            End Get
        End Property

        Private ReadOnly Property RootNode As XmlNode
            Get
                Return Me.XMLDoc.ChildNodes(0)
            End Get
        End Property

        Public ReadOnly Property ParentNode As DNNNode
            Get
                Return If(((Me.XmlNode.ParentNode Is Nothing) OrElse (Me.XmlNode.ParentNode.NodeType = XmlNodeType.Document)), Nothing, New DNNNode(Me.XmlNode.ParentNode))
            End Get
        End Property

        Public ReadOnly Property DNNNodes As DNNNodeCollection
            Get
                If Object.ReferenceEquals(Me.m_objNodes, Nothing) Then
                    Me.m_objNodes = New DNNNodeCollection(Me.XmlNode)
                End If
                Return Me.m_objNodes
            End Get
        End Property

        Public ReadOnly Property XmlNodes As XmlNodeList
            Get
                Return Me.XmlNode.ChildNodes
            End Get
        End Property

        Public Property HasNodes As Boolean
            Get
                Dim flag As Boolean = Conversions.ToBoolean(Me.CustomAttribute("hasNodes", "0"))
                Return If(flag, flag, (Me.DNNNodes.Count > 0))
            End Get
            Set(ByVal Value As Boolean)
                Me.CustomAttribute("hasNodes") = Conversions.ToString(Interaction.IIf(Value, "1", "0"))
            End Set
        End Property

        Public ReadOnly Property ParentNameSpace As String
            Get
                If (Strings.Len(Me.m_strParentNS) = 0) Then
                    Me.m_strParentNS = ""
                    If (If(((Me.XmlNode.ParentNode Is Nothing) OrElse (Not TypeOf Me.XmlNode.ParentNode Is XmlElement OrElse (Me.XmlNode.ParentNode.Attributes.GetNamedItem("id") Is Nothing))), 0, 1) <> 0) Then
                        Me.m_strParentNS = Me.XmlNode.ParentNode.Attributes.GetNamedItem("id").Value
                    End If
                End If
                Return Me.m_strParentNS
            End Get
        End Property

        Public ReadOnly Property Level As Integer
            Get
                Dim num As Integer
                If Object.ReferenceEquals(Me.ParentNode, Nothing) Then
                    num = -1
                Else
                    Dim xmlNode As XmlNode = Me.XmlNode
                    Dim num2 As Integer = -1
                    Do While True
                        If ((Not xmlNode Is Nothing) AndAlso TypeOf xmlNode Is XmlElement) Then
                            num2 += 1
                            xmlNode = xmlNode.ParentNode
                            If (If(((xmlNode Is Nothing) OrElse (xmlNode.Name <> "root")), 0, 1) = 0) Then
                                Continue Do
                            End If
                        End If
                        num = num2
                        Exit Do
                    Loop
                End If
                Return num
            End Get
        End Property

        Public Property CustomAttribute(ByVal Key As String) As String
            Get
                Return If(Object.ReferenceEquals(Me.XmlNode.Attributes.GetNamedItem(Key), Nothing), Nothing, Me.XmlNode.Attributes.GetNamedItem(Key).Value)
            End Get
            Set(ByVal Value As String)
                Try 
                    If Object.ReferenceEquals(Me.XmlNode.Attributes.GetNamedItem(Key), Nothing) Then
                        If Not Object.ReferenceEquals(Value, Nothing) Then
                            Dim node As XmlAttribute = Me.XMLDoc.CreateAttribute(Key)
                            node.Value = Value
                            Me.XmlNode.Attributes.Append(node)
                        End If
                    ElseIf Object.ReferenceEquals(Value, Nothing) Then
                        Me.XmlNode.Attributes.Remove(DirectCast(Me.XmlNode.Attributes.GetNamedItem(Key), XmlAttribute))
                    Else
                        Me.XmlNode.Attributes.GetNamedItem(Key).Value = Value
                    End If
                Catch exception1 As Exception
                    Dim ex As Exception = exception1
                    ProjectData.SetProjectError(ex)
                    Throw ex
                End Try
            End Set
        End Property

        Public Property CustomAttribute(ByVal Key As String, ByVal DefaultValue As String) As String
            Get
                Return If((Strings.Len(Me.CustomAttribute(Key)) <> 0), Me.CustomAttribute(Key), DefaultValue)
            End Get
            Set(ByVal Value As String)
                If (Value = DefaultValue) Then
                    Value = ""
                End If
                Me.CustomAttribute(Key) = Value
            End Set
        End Property

        Public Property ID As String
            Get
                Return Me.CustomAttribute("id")
            End Get
            Set(ByVal Value As String)
                Me.CustomAttribute("id") = Value
            End Set
        End Property

        Public ReadOnly Property ClientID As String
            Get
                Return Me.ID.Replace(":", "_")
            End Get
        End Property

        Public Property Key As String
            Get
                Return Me.CustomAttribute("key")
            End Get
            Set(ByVal Value As String)
                Me.CustomAttribute("key") = Value
            End Set
        End Property

        Public Property [Text] As String
            Get
                Return Me.CustomAttribute("txt")
            End Get
            Set(ByVal Value As String)
                Me.CustomAttribute("txt") = Value
            End Set
        End Property

        Public Property NavigateURL As String
            Get
                Return Me.CustomAttribute("url")
            End Get
            Set(ByVal Value As String)
                If (Strings.Len(Value) <= 0) Then
                    Me.CustomAttribute("url") = Nothing
                Else
                    Me.ClickAction = eClickAction.Navigate
                    Me.CustomAttribute("url") = Value
                End If
            End Set
        End Property

        Public Property JSFunction As String
            Get
                Return Me.CustomAttribute("js")
            End Get
            Set(ByVal Value As String)
                Me.CustomAttribute("js") = Value
            End Set
        End Property

        Public Property Target As String
            Get
                Return Me.CustomAttribute("tar")
            End Get
            Set(ByVal Value As String)
                Me.CustomAttribute("tar") = Value
            End Set
        End Property

        Public Property ToolTip As String
            Get
                Return Me.CustomAttribute("tTip")
            End Get
            Set(ByVal Value As String)
                Me.CustomAttribute("tTip") = If((Strings.Len(Value) <> 0), Value, Nothing)
            End Set
        End Property

        Public Property Enabled As Boolean
            Get
                Return Conversions.ToBoolean(Me.CustomAttribute("enabled", "1"))
            End Get
            Set(ByVal Value As Boolean)
                Me.CustomAttribute("enabled") = Conversions.ToString(Interaction.IIf(Value, "1", "0"))
            End Set
        End Property

        Public Property CSSClass As String
            Get
                Return Me.CustomAttribute("css")
            End Get
            Set(ByVal Value As String)
                Me.CustomAttribute("css") = Value
            End Set
        End Property

        Public Property CSSClassSelected As String
            Get
                Return Me.CustomAttribute("cssSel")
            End Get
            Set(ByVal Value As String)
                Me.CustomAttribute("cssSel") = Value
            End Set
        End Property

        Public Property CSSClassHover As String
            Get
                Return Me.CustomAttribute("cssHover")
            End Get
            Set(ByVal Value As String)
                Me.CustomAttribute("cssHover") = Value
            End Set
        End Property

        Public Property CSSIcon As String
            Get
                Return Me.CustomAttribute("cssIcon")
            End Get
            Set(ByVal Value As String)
                Me.CustomAttribute("cssIcon") = Value
            End Set
        End Property

        Public Property Image As String
            Get
                Return Me.CustomAttribute("img")
            End Get
            Set(ByVal Value As String)
                Me.CustomAttribute("img") = Value
            End Set
        End Property

        Public Property LargeImage As String
            Get
                Return Me.CustomAttribute("largeImg")
            End Get
            Set(ByVal Value As String)
                Me.CustomAttribute("largeImg") = Value
            End Set
        End Property

        Public Property Selected As Boolean
            Get
                Return Conversions.ToBoolean(Me.CustomAttribute("selected", "0"))
            End Get
            Set(ByVal Value As Boolean)
                Me.CustomAttribute("selected") = Conversions.ToString(Interaction.IIf(Value, "1", "0"))
            End Set
        End Property

        Public Property BreadCrumb As Boolean
            Get
                Return Conversions.ToBoolean(Me.CustomAttribute("bcrumb", "0"))
            End Get
            Set(ByVal Value As Boolean)
                Me.CustomAttribute("bcrumb") = Conversions.ToString(Interaction.IIf(Value, "1", "0"))
            End Set
        End Property

        Public Property ClickAction As eClickAction
            Get
                Return If((Strings.Len(Me.CustomAttribute("ca")) <= 0), eClickAction.PostBack, DirectCast(Conversions.ToInteger(Me.CustomAttribute("ca")), eClickAction))
            End Get
            Set(ByVal Value As eClickAction)
                Me.CustomAttribute("ca") = Conversions.ToString(CInt(Value))
            End Set
        End Property

        Public Property IsBreak As Boolean
            Get
                Return Conversions.ToBoolean(Me.CustomAttribute("isBreak", "0"))
            End Get
            Set(ByVal Value As Boolean)
                Me.CustomAttribute("isBreak") = Conversions.ToString(Interaction.IIf(Value, "1", "0"))
            End Set
        End Property


        ' Fields
        Private m_objXMLDoc As XmlDocument
        Private m_objXMLNode As XmlNode
        Private m_strParentNS As String
        Private m_objNodes As DNNNodeCollection
        Private m_objHashAttributes As Hashtable
    End Class
End Namespace

