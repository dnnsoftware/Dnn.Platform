' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.Globalization
Imports System.Reflection
Imports System.Text

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public NotInheritable Class TabStripTabCollection
        Implements IList, ICollection, IEnumerable
        ' Methods
        Friend Sub New(ByVal owner As DNNTabStrip)
            Me.owner = owner
        End Sub

        Public Function Add(ByVal row As DNNTab) As Integer
            Me.AddAt(-1, row)
            Return (Me.owner.Controls.Count - 1)
        End Function

        Private Function Add(ByVal o As Object) As Integer Implements IList.Add
            Return Me.Add(DirectCast(o, DNNTab))
        End Function

        Public Sub AddAt(ByVal index As Integer, ByVal row As DNNTab)
            Me.owner.Controls.AddAt(index, row)
            If (Strings.Len(row.ID) = 0) Then
                row.ID = Me.owner.Controls.Count.ToString(CultureInfo.InvariantCulture)
            End If
        End Sub

        Public Sub AddRange(ByVal rows As DNNTab())
            If (rows Is Nothing) Then
                Throw New ArgumentNullException(NameOf(rows))
            End If
            Dim tab As DNNTab
            For Each tab In rows
                Me.Add(tab)
            Next
        End Sub

        Public Sub Clear() Implements IList.Clear
            If Me.owner.HasControls Then
                Me.owner.Controls.Clear
            End If
        End Sub

        Private Function Contains(ByVal o As Object) As Boolean Implements IList.Contains
            Return Me.owner.Controls.Contains(DirectCast(o, DNNTab))
        End Function

        Public Sub CopyTo(ByVal array As Array, ByVal index As Integer) Implements ICollection.CopyTo
            Dim enumerator As IEnumerator = Me.GetEnumerator
            Do While enumerator.MoveNext
                index += 1
                array.SetValue(enumerator.Current, index)
            Loop
        End Sub

        Public Function FindTab(ByVal strId As String) As DNNTab
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As DNNTab = DirectCast(enumerator.Current, DNNTab)
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

        Public Function GetRowIndex(ByVal row As DNNTab) As Integer
            Return If(Not Me.owner.HasControls, -1, Me.owner.Controls.IndexOf(row))
        End Function

        Private Function IndexOf(ByVal o As Object) As Integer Implements IList.IndexOf
            Return Me.owner.Controls.IndexOf(DirectCast(o, DNNTab))
        End Function

        Private Sub Insert(ByVal index As Integer, ByVal o As Object) Implements IList.Insert
            Me.AddAt(index, DirectCast(o, DNNTab))
        End Sub

        Public Sub Remove(ByVal row As DNNTab)
            Me.owner.Controls.Remove(row)
        End Sub

        Private Sub Remove(ByVal o As Object) Implements IList.Remove
            Me.Remove(DirectCast(o, DNNTab))
        End Sub

        Public Sub RemoveAt(ByVal index As Integer) Implements IList.RemoveAt
            Me.owner.Controls.RemoveAt(index)
        End Sub

        Public Function ToJSON() As String
            Dim flag As Boolean
            Dim enumerator As IEnumerator = Nothing
            Dim enumerator2 As IEnumerator = Nothing
            Dim builder As New StringBuilder
            builder.Append("tabs:[")
            Try 
                enumerator = Me.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As DNNTab = DirectCast(enumerator.Current, DNNTab)
                    If flag Then
                        builder.Append(","c)
                    End If
                    flag = True
                    builder.Append(ScriptGenerator.GetMarshalledPropertyJSON(current, current.MarshalledProperties))
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            builder.Append("]"c)
            flag = False
            builder.Append(",tablabels:[")
            Try 
                enumerator2 = Me.GetEnumerator
                Do While True
                    If Not enumerator2.MoveNext Then
                        Exit Do
                    End If
                    Dim current As DNNTab = DirectCast(enumerator2.Current, DNNTab)
                    If flag Then
                        builder.Append(","c)
                    End If
                    flag = True
                    builder.Append(ScriptGenerator.GetMarshalledPropertyJSON(current.Label))
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator2,IDisposable), Nothing) Then
                    TryCast(enumerator2,IDisposable).Dispose
                End If
            End Try
            builder.Append("]"c)
            Return builder.ToString
        End Function


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

        Public ReadOnly Default Property Item(ByVal index As Integer) As DNNTab
            Get
                Return DirectCast(Me.owner.Controls(index), DNNTab)
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
                Me.AddAt(index, DirectCast(value, DNNTab))
            End Set
        End Property

        ' Fields
        Private owner As DNNTabStrip
    End Class
End Namespace

