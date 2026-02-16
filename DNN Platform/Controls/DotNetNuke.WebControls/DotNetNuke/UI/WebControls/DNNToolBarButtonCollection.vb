' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Reflection

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public Class DNNToolBarButtonCollection
        Implements IList, ICollection, IEnumerable
        ' Methods
        Friend Sub New(ByVal owner As DNNToolBar)
            Me.owner = owner
        End Sub

        Public Function Add(ByVal row As DNNToolBarButton) As Integer
            Me.AddAt(-1, row)
            Return (Me.owner.Controls.Count - 1)
        End Function

        Private Function Add(ByVal o As Object) As Integer Implements IList.Add
            Return Me.Add(DirectCast(o, DNNToolBarButton))
        End Function

        Public Sub AddAt(ByVal index As Integer, ByVal row As DNNToolBarButton)
            Me.owner.Controls.AddAt(index, row)
        End Sub

        Public Sub AddRange(ByVal rows As DNNToolBarButton())
            If (rows Is Nothing) Then
                Throw New ArgumentNullException(NameOf(rows))
            End If
            Dim button As DNNToolBarButton
            For Each button In rows
                Me.Add(button)
            Next
        End Sub

        Public Sub Clear() Implements IList.Clear
            If Me.owner.HasControls Then
                Me.owner.Controls.Clear
            End If
        End Sub

        Private Function Contains(ByVal o As Object) As Boolean Implements IList.Contains
            Return Me.owner.Controls.Contains(DirectCast(o, DNNToolBarButton))
        End Function

        Public Sub CopyTo(ByVal array As Array, ByVal index As Integer) Implements ICollection.CopyTo
            Dim enumerator As IEnumerator = Me.GetEnumerator
            Do While enumerator.MoveNext
                index += 1
                array.SetValue(enumerator.Current, index)
            Loop
        End Sub

        Public Function FindTab(ByVal strId As String) As DNNToolBarButton
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As DNNToolBarButton = DirectCast(enumerator.Current, DNNToolBarButton)
                    If (current.ID = strId) Then
                        Return current
                    End If
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            Return Nothing
        End Function

        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Me.owner.Controls.GetEnumerator
        End Function

        Public Function GetRowIndex(ByVal row As DNNToolBarButton) As Integer
            Return If(Not Me.owner.HasControls, -1, Me.owner.Controls.IndexOf(row))
        End Function

        Private Function IndexOf(ByVal o As Object) As Integer Implements IList.IndexOf
            Return Me.owner.Controls.IndexOf(DirectCast(o, DNNToolBarButton))
        End Function

        Private Sub Insert(ByVal index As Integer, ByVal o As Object) Implements IList.Insert
            Me.AddAt(index, DirectCast(o, DNNToolBarButton))
        End Sub

        Public Sub Remove(ByVal row As DNNToolBarButton)
            Me.owner.Controls.Remove(row)
        End Sub

        Private Sub Remove(ByVal o As Object) Implements IList.Remove
            Me.Remove(DirectCast(o, DNNToolBarButton))
        End Sub

        Public Sub RemoveAt(ByVal index As Integer) Implements IList.RemoveAt
            Me.owner.Controls.RemoveAt(index)
        End Sub


        ' Properties
        Public ReadOnly Property Count As Integer Implements ICollection.Count
            Get
                Return If(Not Me.owner.HasControls, 0, Me.owner.Controls.Count)
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements IList.IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property IsSynchronized As Boolean Implements ICollection.IsSynchronized
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Default Property Item(ByVal index As Integer) As DNNToolBarButton
            Get
                Return DirectCast(Me.owner.Controls(index), DNNToolBarButton)
            End Get
        End Property

        Public ReadOnly Property SyncRoot As Object Implements ICollection.SyncRoot
            Get
                Return Me
            End Get
        End Property

        Private ReadOnly Property IsFixedSize As Boolean Implements IList.IsFixedSize
            Get
                Return False
            End Get
        End Property

        Private Property _item(ByVal index As Integer) As Object Implements IList.Item
            Get
                Return Me.owner.Controls(index)
            End Get
            Set(ByVal value As Object)
                Me.RemoveAt(index)
                Me.AddAt(index, DirectCast(value, DNNToolBarButton))
            End Set
        End Property


        ' Fields
        Private owner As DNNToolBar
    End Class
End Namespace

