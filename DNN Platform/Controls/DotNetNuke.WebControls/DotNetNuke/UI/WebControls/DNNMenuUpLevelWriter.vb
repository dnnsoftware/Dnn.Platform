' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class DNNMenuUpLevelWriter
        Inherits WebControl
        Implements IDNNMenuWriter
        ' Methods
        Public Sub New()
            DNNMenuUpLevelWriter.__ENCAddToList(Me)
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
            SyncLock DNNMenuUpLevelWriter.__ENCList
                If (DNNMenuUpLevelWriter.__ENCList.Count = DNNMenuUpLevelWriter.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNMenuUpLevelWriter.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNMenuUpLevelWriter.__ENCList.RemoveRange(index, (DNNMenuUpLevelWriter.__ENCList.Count - index))
                            DNNMenuUpLevelWriter.__ENCList.Capacity = DNNMenuUpLevelWriter.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNMenuUpLevelWriter.__ENCList(num2)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNMenuUpLevelWriter.__ENCList(index) = DNNMenuUpLevelWriter.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNMenuUpLevelWriter.__ENCList.Add(New WeakReference(value))
            End SyncLock
        End Sub

        Protected Overrides Sub RenderChildren(ByVal writer As HtmlTextWriter)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.m_Menu.MenuNodes.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    DirectCast(enumerator.Current, MenuNode).Render(writer)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
        End Sub

        Protected Overrides Sub RenderContents(ByVal writer As HtmlTextWriter)
            writer.AddAttribute(HtmlTextWriterAttribute.Class, Me.m_Menu.CssClass)
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Me.m_Menu.ClientID)
            writer.RenderBeginTag(HtmlTextWriterTag.Span)
            writer.RenderEndTag
        End Sub

        Public Sub RenderMenu(ByVal writer As HtmlTextWriter, ByVal Menu As DNNMenu) Implements IDNNMenuWriter.RenderMenu
            Me.m_Menu = Menu
            Me.RenderControl(writer)
        End Sub


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private m_Menu As DNNMenu
    End Class
End Namespace

