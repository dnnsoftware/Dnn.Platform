' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections

Namespace DotNetNuke.UI.Utilities

    Public Class FunctionalityCollection

        Inherits CollectionBase
        Implements IList(Of FunctionalityInfo)

        ''' <inheritdoc />
        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of FunctionalityInfo).IsReadOnly
            Get
                Return Me.InnerList.IsReadOnly
            End Get
        End Property
        
        ''' <inheritdoc />
        Private ReadOnly Property Generic_Count As Integer Implements ICollection(Of FunctionalityInfo).Count
            Get
                Return MyBase.Count
            End Get
        End Property

        Public Sub Add(ByVal f As FunctionalityInfo) Implements ICollection(Of FunctionalityInfo).Add
            Me.InnerList.Add(f)
        End Sub

        Default Public Overridable Property Item(ByVal index As Integer) As FunctionalityInfo
            Get
                Return DirectCast(MyBase.List.Item(index), FunctionalityInfo)
            End Get
            Set(ByVal Value As FunctionalityInfo)
                MyBase.List.Item(index) = Value
            End Set
        End Property

        ''' <inheritdoc />
        Private Property IList_Item(index As Integer) As FunctionalityInfo Implements IList(Of FunctionalityInfo).Item
            Get
                Return Me(index)
            End Get
            Set
                Me(index) = Value
            End Set
        End Property

        Default Public Overridable Property Item(ByVal functionality As ClientAPI.ClientFunctionality) As FunctionalityInfo
            Get
                Dim _fInfo As FunctionalityInfo = Nothing
                For Each fInfo As FunctionalityInfo In List
                    If fInfo.Functionality = functionality Then
                        _fInfo = fInfo
                        Exit For
                    End If
                Next
                Return _fInfo
            End Get
            Set(ByVal value As FunctionalityInfo)
                Dim bFound As Boolean = False
                For Each fInfo As FunctionalityInfo In List
                    If fInfo.Functionality = functionality Then
                        fInfo = value
                        bFound = True
                        Exit For
                    End If
                Next
                If Not bFound Then
                    Throw New KeyNotFoundException("Item Not Found")
                End If
            End Set
        End Property

        ''' <inheritdoc />
        Public Function Remove(item As FunctionalityInfo) As Boolean Implements ICollection(Of FunctionalityInfo).Remove
            If Not Me.Contains(item) Then
                Return False
            End If

            Me.InnerList.Remove(item)
            Return True
        End Function

        ''' <inheritdoc />
        Public Function Contains(item As FunctionalityInfo) As Boolean Implements ICollection(Of FunctionalityInfo).Contains
            Return Me.InnerList.Contains(item)
        End Function

        ''' <inheritdoc />
        Public Sub CopyTo(array As FunctionalityInfo(), arrayIndex As Integer) Implements ICollection(Of FunctionalityInfo).CopyTo
            Me.InnerList.CopyTo(array, arrayIndex)
        End Sub

        ''' <inheritdoc />
        Public Function IndexOf(item As FunctionalityInfo) As Integer Implements IList(Of FunctionalityInfo).IndexOf
            Return Me.InnerList.IndexOf(item)
        End Function

        ''' <inheritdoc />
        Public Sub Insert(index As Integer, item As FunctionalityInfo) Implements IList(Of FunctionalityInfo).Insert
            Me.InnerList.Insert(index, item)
        End Sub

        ''' <inheritdoc />
        Private Sub Generic_Clear() Implements ICollection(Of FunctionalityInfo).Clear
            MyBase.Clear()
        End Sub

        ''' <inheritdoc />
        Private Sub Generic_RemoveAt(index As Integer) Implements IList(Of FunctionalityInfo).RemoveAt
            MyBase.RemoveAt(index)
        End Sub

        ''' <inheritdoc />
        Private Function Generic_GetEnumerator() As IEnumerator(Of FunctionalityInfo) Implements IEnumerable(Of FunctionalityInfo).GetEnumerator
            Return New FunctionalityInfoEnumerator(Me.GetEnumerator())
        End Function

        Private NotInheritable Class FunctionalityInfoEnumerator
            Implements IEnumerator(Of FunctionalityInfo)

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
            Public ReadOnly Property Generic_Current As FunctionalityInfo Implements IEnumerator(Of FunctionalityInfo).Current
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
