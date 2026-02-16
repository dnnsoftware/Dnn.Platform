' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Reflection
Imports System.Xml

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public Class MenuNodeCollection
        Inherits DNNNodeCollection
        ' Methods
        Public Sub New(ByVal JSONDict As Dictionary(Of String, Object), ByVal Menu As DNNMenu)
            MyBase.New(Menu.ClientID, JSONDict)
            Me.m_objDNNMenu = Menu
        End Sub

        Public Sub New(ByVal strNamespace As String, ByVal objControl As DNNMenu)
            MyBase.New(strNamespace)
            Me.m_objDNNMenu = objControl
        End Sub

        Public Sub New(ByVal objXmlNode As XmlNode, ByVal objControl As DNNMenu)
            MyBase.New(objXmlNode)
            Me.m_objDNNMenu = objControl
        End Sub

        Public Sub New(ByVal strXML As String, ByVal strXSLFile As String, ByVal objControl As DNNMenu)
            MyBase.New(strXML, strXSLFile)
            Me.m_objDNNMenu = objControl
        End Sub

        Public Shadows Function Add() As Integer
            Return Me.Add("")
        End Function

        Public Shadows Function Add(ByVal objNode As MenuNode) As Integer
            Dim num2 As Integer = MyBase.Add(objNode)
            objNode.SetDNNMenu(Me.m_objDNNMenu)
            If (Strings.Len(objNode.ID) = 0) Then
                objNode.ID = (objNode.ParentNameSpace & "_" & Conversions.ToString(objNode.GetHashCode))
            End If
            Return num2
        End Function

        Public Shadows Function Add(ByVal strText As String) As Integer
            Dim objNode As New MenuNode
            Dim num2 As Integer = Me.Add(objNode)
            objNode.Text = strText
            Return num2
        End Function

        Public Shadows Function Add(ByVal strID As String, ByVal strKey As String, ByVal strText As String, ByVal strNavigateURL As String) As Integer
            Return Me.Add(strID, strKey, strText, strNavigateURL, "", "", "", True, "", "", "", False, eClickAction.Navigate, "", -1)
        End Function

        Public Shadows Function Add(ByVal strID As String, ByVal strKey As String, ByVal strText As String, ByVal strNavigateURL As String, ByVal strJSFunction As String, ByVal strTarget As String, ByVal strToolTip As String, ByVal blnEnabled As Boolean, ByVal strCSSClass As String, ByVal strCSSClassSelected As String, ByVal strCSSClassHover As String, ByVal blnSelected As Boolean, ByVal enumClickAction As eClickAction, ByVal strCssClassOver As String, ByVal intImageIndex As Integer) As Integer
            Dim num2 As Integer = Me.Add
            Dim node As MenuNode = Me(num2)
            node.ID = If((Strings.Len(strID) <= 0), (node.ParentNameSpace & "_" & Conversions.ToString(Me.XMLNode.ChildNodes.Count)), strID)
            node.Key = strKey
            node.Text = strText
            node.NavigateURL = strNavigateURL
            node.JSFunction = strJSFunction
            node.Target = strTarget
            node.ToolTip = strToolTip
            node.Enabled = blnEnabled
            node.CSSClass = strCSSClass
            node.CSSClassSelected = strCSSClassSelected
            node.CSSClassHover = strCSSClassHover
            node.Selected = blnSelected
            node.ClickAction = enumClickAction
            node.CssClassOver = strCssClassOver
            node.ImageIndex = intImageIndex
            Return num2
        End Function

        Public Shadows Function Contains(ByVal value As MenuNode) As Boolean
            Return Not Object.ReferenceEquals(Me.FindNode(value.ID), Nothing)
        End Function

        Public Shadows Function FindNode(ByVal ID As String) As MenuNode
            Dim objA As XmlNode = Me.FindFast("id", ID, Me.XMLNode, True)
            Return If(Object.ReferenceEquals(objA, Nothing), Nothing, New MenuNode(objA, Me.m_objDNNMenu))
        End Function

        Public Shadows Function FindNodeByKey(ByVal Key As String) As MenuNode
            Dim objA As XmlNode = Me.FindFast("key", Key, Me.XMLNode, True)
            Return If(Object.ReferenceEquals(objA, Nothing), Nothing, New MenuNode(objA, Me.m_objDNNMenu))
        End Function

        Public Overrides Function FindSelectedNodes() As Collection
            Dim collection As New Collection
            Dim flag As Boolean = Not Object.ReferenceEquals(Me.XMLNode, Nothing)
            If flag Then
                Dim enumerator As IEnumerator = Nothing
                Dim list As XmlNodeList = Me.XMLNode.SelectNodes("//n[@selected='1']")
                Try 
                    enumerator = list.GetEnumerator
                    Do While True
                        flag = enumerator.MoveNext
                        If Not flag Then
                            Exit Do
                        End If
                        Dim current As XmlNode = DirectCast(enumerator.Current, XmlNode)
                        collection.Add(New MenuNode(current, Me.m_objDNNMenu), Nothing, Nothing, Nothing)
                    Loop
                Finally
                    If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                        TryCast(enumerator,IDisposable).Dispose
                    End If
                End Try
            End If
            Return collection
        End Function

        Public Shadows Function GetEnumerator() As IEnumerator
            Return New MenuNodeEnumerator(Me.XMLNode, Me.m_objDNNMenu)
        End Function

        Public Shadows Function IndexOf(ByVal value As MenuNode) As Integer
            Dim count As Integer = Me.XMLNode.ChildNodes.Count
            Dim num As Integer = 0
            Do While True
                Dim num2 As Integer
                Dim num4 As Integer = count
                If (num <= num4) Then
                    If (New MenuNode(Me.XMLNode.ChildNodes(num), Me.m_objDNNMenu).ID <> value.ID) Then
                        num += 1
                        Continue Do
                    End If
                    num2 = num
                End If
                Return num2
            Loop

            Return Nothing
        End Function

        Public Sub Insert(ByVal index As Integer, ByVal value As MenuNode)
            Me.XMLNode.InsertAfter(Me.XMLNode.ChildNodes(index), value.XmlNode)
        End Sub

        Public Shadows Sub Remove(ByVal value As MenuNode)
            Me.XMLNode.RemoveChild(value.XmlNode)
        End Sub


        ' Properties
        Public Default Shadows Property Item(ByVal index As Integer) As MenuNode
            Get
                Return New MenuNode(Me.XMLNode.ChildNodes(index), Me.m_objDNNMenu)
            End Get
            Set(ByVal Value As MenuNode)
                Throw New NotSupportedException("Cannot Assign Node Directly")
            End Set
        End Property


        ' Fields
        Private m_objDNNMenu As DNNMenu
    End Class
End Namespace

