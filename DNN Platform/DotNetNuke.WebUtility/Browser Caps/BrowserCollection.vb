' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System.Collections

Namespace DotNetNuke.UI.Utilities

    Public Class BrowserCollection

        Inherits CollectionBase
        Implements IList(Of Browser)

        ''' <inheritdoc />
        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of Browser).IsReadOnly
            Get
                Return Me.InnerList.IsReadOnly
            End Get
        End Property
        
        ''' <inheritdoc />
        Private ReadOnly Property Generic_Count As Integer Implements ICollection(Of Browser).Count
            Get
                Return MyBase.Count
            End Get
        End Property

        Default Public Overridable Property Item(ByVal index As Integer) As Browser
            Get
                Return DirectCast(MyBase.List.Item(index), Browser)
            End Get
            Set(ByVal Value As Browser)
                MyBase.List.Item(index) = Value
            End Set
        End Property

        ''' <inheritdoc />
        Private Property IList_Item(index As Integer) As Browser Implements IList(Of Browser).Item
            Get
                Return Me(index)
            End Get
            Set
                Me(index) = Value
            End Set
        End Property

        ''' <inheritdoc />
        Public Function Remove(item As Browser) As Boolean Implements ICollection(Of Browser).Remove
            If Not Me.Contains(item) Then
                Return False
            End If

            Me.InnerList.Remove(item)
            Return True
        End Function

        ''' <inheritdoc />
        Public Sub Add(ByVal b As Browser) Implements ICollection(Of Browser).Add
            Me.InnerList.Add(b)
        End Sub

        ''' <inheritdoc />
        Public Function Contains(item As Browser) As Boolean Implements ICollection(Of Browser).Contains
            Return Me.InnerList.Contains(item)
        End Function

        ''' <inheritdoc />
        Public Sub CopyTo(array As Browser(), arrayIndex As Integer) Implements ICollection(Of Browser).CopyTo
            Me.InnerList.CopyTo(array, arrayIndex)
        End Sub

        ''' <inheritdoc />
        Public Function IndexOf(item As Browser) As Integer Implements IList(Of Browser).IndexOf
            Return Me.InnerList.IndexOf(item)
        End Function

        ''' <inheritdoc />
        Public Sub Insert(index As Integer, item As Browser) Implements IList(Of Browser).Insert
            Me.InnerList.Insert(index, item)
        End Sub

        ''' <inheritdoc />
        Private Sub Generic_Clear() Implements ICollection(Of Browser).Clear
            MyBase.Clear()
        End Sub

        ''' <inheritdoc />
        Private Sub Generic_RemoveAt(index As Integer) Implements IList(Of Browser).RemoveAt
            MyBase.RemoveAt(index)
        End Sub

        ''' <inheritdoc />
        Private Function Generic_GetEnumerator() As IEnumerator(Of Browser) Implements IEnumerable(Of Browser).GetEnumerator
            Return New BrowserEnumerator(Me.GetEnumerator())
        End Function

        Private NotInheritable Class BrowserEnumerator
            Implements IEnumerator(Of Browser)

            Private ReadOnly enumerator As IEnumerator

            Public Sub New(enumerator As IEnumerator)
                Me.enumerator = enumerator
            End Sub

            ''' <inheritdoc />
            Public Sub Dispose() Implements IDisposable.Dispose
                Dim disposable = TryCast(Me.enumerator, IDisposable)
                If disposable IsNot nothing then
                    disposable.Dispose()
                End If
            End Sub

            ''' <inheritdoc />
            Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
                Return Me.enumerator.MoveNext()
            End Function

            ''' <inheritdoc />
            Public Sub Reset() Implements IEnumerator.Reset
                Me.enumerator.Reset()
            End Sub

            ''' <inheritdoc />
            Public ReadOnly Property Generic_Current As Browser Implements IEnumerator(Of Browser).Current
                Get
                    Return Me.enumerator.Current
                End Get
            End Property

            ''' <inheritdoc />
            Public ReadOnly Property Current As Object Implements IEnumerator.Current
                Get
                    Return Me.Generic_Current
                End Get
            End Property

        End Class

    End Class

End Namespace
