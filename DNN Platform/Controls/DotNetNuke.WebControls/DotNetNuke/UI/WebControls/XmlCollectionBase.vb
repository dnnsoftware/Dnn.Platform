' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.IO
Imports System.Xml

Namespace DotNetNuke.UI.WebControls
    Public MustInherit Class XmlCollectionBase
        Implements ICollection, IList, IEnumerable
        ' Methods
        Protected Sub New()
        End Sub

        Protected Sub New(ByVal objTreeControl As DnnTree)
            Me.m_objTree = objTreeControl
        End Sub

        Public Sub New(ByVal strNamespace As String)
            Me.InnerXMLDoc = New XmlDocument
            Me.InnerXMLNode = Me.InnerXMLDoc.CreateNode(XmlNodeType.Element, "root", "")
            Dim node As XmlAttribute = Me.InnerXMLDoc.CreateAttribute("id")
            node.Value = strNamespace
            Me.InnerXMLNode.Attributes.Append(node)
            Me.InnerXMLDoc.AppendChild(Me.InnerXMLNode)
        End Sub

        Public Sub New(ByVal objXmlNode As XmlNode)
            Me.InnerXMLNode = objXmlNode
            Me.InnerXMLDoc = Me.InnerXMLNode.OwnerDocument
        End Sub

        Public Sub New(ByVal strNamespace As String, ByVal objTreeControl As DnnTree)
            Me.m_objTree = objTreeControl
            Me.InnerXMLDoc = New XmlDocument
            Me.InnerXMLNode = Me.InnerXMLDoc.CreateNode(XmlNodeType.Element, "root", "")
            Dim node As XmlAttribute = Me.InnerXMLDoc.CreateAttribute("id")
            node.Value = strNamespace
            Me.InnerXMLNode.Attributes.Append(node)
            Me.InnerXMLDoc.AppendChild(Me.InnerXMLNode)
        End Sub

        Public Sub New(ByVal strXML As String, ByVal strXSLFile As String)
            Me.InnerXMLDoc = New XmlDocument With { .XmlResolver = Nothing }
            If (Strings.Len(strXSLFile) <= 0) Then
                Using stringReader = New StringReader(strXML)
                Using xmlReader As XmlReader = XmlReader.Create(stringReader, New XmlReaderSettings With { .XmlResolver = Nothing })
                    Me.InnerXMLDoc.Load(xmlReader)
                End Using
                End Using
            End If
            Me.InnerXMLNode = Me.InnerXMLDoc.SelectSingleNode("//root")
        End Sub

        Public Sub New(ByVal objXmlNode As XmlNode, ByVal objTreeControl As DnnTree)
            Me.m_objTree = objTreeControl
            Me.InnerXMLNode = objXmlNode
            Me.InnerXMLDoc = Me.InnerXMLNode.OwnerDocument
        End Sub

        Private Function Add(ByVal value As Object) As Integer Implements IList.Add
            Dim num As Integer
            Return num
        End Function

        Public Sub Clear() Implements IList.Clear
            Dim num As Integer = (Me.InnerXMLNode.ChildNodes.Count - 1)
            Do While True
                Dim num2 As Integer = 0
                If (num < num2) Then
                    Return
                End If
                Me.InnerXMLNode.RemoveChild(Me.InnerXMLNode.ChildNodes(num))
                num = (num + -1)
            Loop
        End Sub

        Private Function Contains(ByVal value As Object) As Boolean Implements IList.Contains
            Dim flag As Boolean
            Return flag
        End Function

        Private Sub CopyTo(ByVal array As Array, ByVal index As Integer) Implements ICollection.CopyTo
        End Sub

        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return If(Not Object.ReferenceEquals(Me.m_objTree, Nothing), New TreeNodeEnumerator(Me.m_objXMLNode, Me.m_objTree), Me.m_objXMLNode.ChildNodes.GetEnumerator)
        End Function

        Private Function IndexOf(ByVal value As Object) As Integer Implements IList.IndexOf
            Dim num As Integer
            Return num
        End Function

        Private Sub Insert(ByVal index As Integer, ByVal value As Object) Implements IList.Insert
        End Sub

        Private Sub Remove(ByVal value As Object) Implements IList.Remove
        End Sub

        Public Sub RemoveAt(ByVal index As Integer) Implements IList.RemoveAt
            Me.InnerXMLNode.RemoveChild(Me.InnerXMLNode.ChildNodes(index))
        End Sub


        ' Properties
        Private ReadOnly Property IsSynchronized As Boolean Implements ICollection.IsSynchronized
            Get
                Dim flag As Boolean
                Return flag
            End Get
        End Property

        Private ReadOnly Property SyncRoot As Object Implements ICollection.SyncRoot
            Get
                Dim obj2 As Object = Nothing
                Return obj2
            End Get
        End Property

        Private ReadOnly Property IsFixedSize As Boolean Implements IList.IsFixedSize
            Get
                Dim flag As Boolean
                Return flag
            End Get
        End Property

        Private ReadOnly Property IsReadOnly As Boolean Implements IList.IsReadOnly
            Get
                Dim flag As Boolean
                Return flag
            End Get
        End Property

        Private Property Item(ByVal index As Integer) As Object Implements IList.Item
            Get
                Dim obj2 As Object = Nothing
                Return obj2
            End Get
            Set(ByVal Value As Object)
            End Set
        End Property

        Public ReadOnly Property Count As Integer Implements System.Collections.ICollection.Count
            Get
                Return Me.InnerXMLNode.ChildNodes.Count
            End Get
        End Property

        Protected ReadOnly Property InnerList As ArrayList
            Get
                Return Nothing
            End Get
        End Property

        Protected ReadOnly Property List As IList
            Get
                Return Nothing
            End Get
        End Property

        Protected Property InnerXMLNode As XmlNode
            Get
                Return Me.m_objXMLNode
            End Get
            Set(ByVal Value As XmlNode)
                Me.m_objXMLNode = Value
            End Set
        End Property

        Protected Property InnerXMLDoc As XmlDocument
            Get
                Return Me.m_objXMLDoc
            End Get
            Set(ByVal Value As XmlDocument)
                Me.m_objXMLDoc = Value
            End Set
        End Property

        
        ' Fields
        Private m_arr As ArrayList
        Private m_objXMLNode As XmlNode
        Private m_objXMLDoc As XmlDocument
        Private m_objTree As DnnTree
    End Class
End Namespace

