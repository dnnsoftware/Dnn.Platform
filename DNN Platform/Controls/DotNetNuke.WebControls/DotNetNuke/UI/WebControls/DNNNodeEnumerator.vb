' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Xml

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class DNNNodeEnumerator
        Implements IEnumerator
        ' Methods
        Public Sub New(ByVal objRoot As XmlNode)
            Me.m_objXMLNode = objRoot
            Me.m_intCursor = -1
        End Sub

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            If (Me.m_intCursor < Me.m_objXMLNode.ChildNodes.Count) Then
                Me.m_intCursor += 1
            End If
            Return (Me.m_intCursor <> Me.m_objXMLNode.ChildNodes.Count)
        End Function

        Public Sub Reset() Implements IEnumerator.Reset
            Me.m_intCursor = -1
        End Sub


        ' Properties
        Public ReadOnly Property Current As Object Implements IEnumerator.Current
            Get
                If ((Me.m_intCursor < 0) Or (Me.m_intCursor = Me.m_objXMLNode.ChildNodes.Count)) Then
                    Throw New InvalidOperationException
                End If
                Return New DNNNode(Me.m_objXMLNode.ChildNodes(Me.m_intCursor))
            End Get
        End Property


        ' Fields
        Private m_objXMLNode As XmlNode
        Private m_intCursor As Integer
    End Class
End Namespace

