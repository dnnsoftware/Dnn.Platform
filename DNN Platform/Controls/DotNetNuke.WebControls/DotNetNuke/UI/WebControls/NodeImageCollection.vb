' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Reflection
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public Class NodeImageCollection
        Inherits CollectionBase
        Implements IStateManager
        ' Methods
        Public Sub New()
        End Sub

        Public Sub New(ByVal value As NodeImageCollection)
            Me.AddRange(value)
        End Sub

        Public Sub New(ByVal value As NodeImage())
            Me.AddRange(value)
        End Sub

        Public Function Add(ByVal value As NodeImage) As Integer
            Dim num2 As Integer = Me.List.Add(value)
            If Me.m_isTrackingViewState Then
                value.TrackViewState
                value.SetDirty
            End If
            Return num2
        End Function

        Public Function Add(ByVal ImageUrl As String) As Integer
            Return Me.Add(New NodeImage(ImageUrl))
        End Function

        Public Sub AddRange(ByVal value As NodeImage())
            Dim num2 As Integer = (value.Length - 1)
            Dim index As Integer = 0
            Do While True
                Dim num3 As Integer = num2
                If (index > num3) Then
                    Return
                End If
                Me.Add(value(index))
                index += 1
            Loop
        End Sub

        Public Sub AddRange(ByVal value As NodeImageCollection)
            Dim num2 As Integer = (value.Count - 1)
            Dim num As Integer = 0
            Do While True
                Dim num3 As Integer = num2
                If (num > num3) Then
                    Return
                End If
                Me.Add(DirectCast(value.List(num), NodeImage))
                num += 1
            Loop
        End Sub

        Public Function Contains(ByVal value As NodeImage) As Boolean
            Return Not Object.ReferenceEquals(Me.GetImage(value.ImageUrl), Nothing)
        End Function

        Public Function Contains(ByVal strImageUrl As String) As Boolean
            Return Not Object.ReferenceEquals(Me.GetImage(strImageUrl), Nothing)
        End Function

        Public Sub CopyTo(ByVal array As NodeImage(), ByVal index As Integer)
            Me.List.CopyTo(array, index)
        End Sub

        Private Function GetImage(ByVal strUrl As String) As NodeImage
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.List.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As NodeImage = DirectCast(enumerator.Current, NodeImage)
                    If (strUrl = current.ImageUrl) Then
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

        Private Function GetImageIndex(ByVal strUrl As String) As Integer
            Dim num3 As Integer = (Me.List.Count - 1)
            Dim num2 As Integer = 0
            Do While True
                Dim num As Integer
                Dim num4 As Integer = num3
                If (num2 > num4) Then
                    num = -1
                Else
                    If (strUrl <> DirectCast(Me.List(num2), NodeImage).ImageUrl) Then
                        num2 += 1
                        Continue Do
                    End If
                    num = num2
                End If
                Return num
            Loop

            Return Nothing
        End Function

        Public Function IndexOf(ByVal value As NodeImage) As Integer
            Return Me.GetImageIndex(value.ImageUrl)
        End Function

        Public Function IndexOf(ByVal strImageUrl As String) As Integer
            Return Me.GetImageIndex(strImageUrl)
        End Function

        Public Sub Insert(ByVal index As Integer, ByVal value As NodeImage)
            Me.List.Insert(index, value)
        End Sub

        Public Sub LoadViewState(ByVal state As Object) Implements IStateManager.LoadViewState
            If (Not state Is Nothing) Then
                Dim objArray As Object() = DirectCast(state, Object())
                Dim num2 As Integer = (objArray.Length - 1)
                Dim index As Integer = 0
                Do While True
                    Dim num3 As Integer = num2
                    If (index > num3) Then
                        Exit Do
                    End If
                    Dim image As New NodeImage
                    Me.Add(image)
                    DirectCast(image, IStateManager).TrackViewState
                    DirectCast(image, IStateManager).LoadViewState(objArray(index))
                    index += 1
                Loop
            End If
        End Sub

        Public Sub Remove(ByVal value As NodeImage)
            Me.List.Remove(value)
        End Sub

        Public Function SaveViewState() As Object Implements IStateManager.SaveViewState
            Dim obj2 As Object = Nothing
            If (Me.Count = 0) Then
                obj2 = Nothing
            Else
                Dim objArray As Object() = New Object((Me.Count + 1)  - 1) {}
                Dim num2 As Integer = (Me.Count - 1)
                Dim index As Integer = 0
                Do While True
                    Dim num3 As Integer = num2
                    If (index > num3) Then
                        obj2 = objArray
                        Exit Do
                    End If
                    objArray(index) = Me(index).SaveViewState
                    index += 1
                Loop
            End If
            Return obj2
        End Function

        Public Function ToArray() As NodeImage()
            Dim imageArray As NodeImage() = Nothing
#Disable Warning CA1861
            imageArray = DirectCast(Utils.CopyArray(DirectCast(imageArray, Array), New NodeImage(((Me.Count - 1) + 1)  - 1) {}), NodeImage())
#Enable Warning CA1861
            Me.CopyTo(imageArray, 0)
            Return imageArray
        End Function

        Public Sub TrackViewState() Implements IStateManager.TrackViewState
            Me.m_isTrackingViewState = True
        End Sub


        ' Properties
        Public Default Property Item(ByVal index As Integer) As NodeImage
            Get
                Return DirectCast(Me.List(index), NodeImage)
            End Get
            Set(ByVal Value As NodeImage)
                Me.List(index) = Value
            End Set
        End Property

        Public Property IsTrackingViewState As Boolean Implements IStateManager.IsTrackingViewState
            Get
#Disable Warning BC40000
                Return Me.m_isTrackingViewState
            End Get
            Protected Set(value As Boolean)
                Me.m_isTrackingViewState = value
#Enable Warning BC40000
            End Set
        End Property


        ' Fields
#Disable Warning CA1051
        <Obsolete("Deprecated in DotNetNuke 10.2.3. Please use IsTrackingViewState property. Scheduled removal in v12.0.0.")>
        Protected m_isTrackingViewState As Boolean
#Enable Warning CA1051
    End Class
End Namespace

