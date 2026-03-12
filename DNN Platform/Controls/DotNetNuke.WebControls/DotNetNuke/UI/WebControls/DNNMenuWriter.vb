' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class DNNMenuWriter
        Inherits WebControl
        Implements IDNNMenuWriter
        ' Methods
        Public Sub New()
            DNNMenuWriter.__ENCAddToList(Me)
        End Sub

        Public Sub New(ByVal blnForceFullMenu As Boolean)
            DNNMenuWriter.__ENCAddToList(Me)
            Me.ForceFullMenu = blnForceFullMenu
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
            SyncLock DNNMenuWriter.__ENCList
                If (DNNMenuWriter.__ENCList.Count = DNNMenuWriter.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNMenuWriter.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNMenuWriter.__ENCList.RemoveRange(index, (DNNMenuWriter.__ENCList.Count - index))
                            DNNMenuWriter.__ENCList.Capacity = DNNMenuWriter.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNMenuWriter.__ENCList(num2)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNMenuWriter.__ENCList(index) = DNNMenuWriter.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNMenuWriter.__ENCList.Add(New WeakReference(value))
            End SyncLock
        End Sub

        Protected Overrides Sub RenderChildren(ByVal writer As HtmlTextWriter)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.m_objMenu.MenuNodes.GetEnumerator
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
            Dim str As String = "DNNMenu "
            If (Me.m_objMenu.SelectedMenuNodes.Count = 0) Then
                If (Strings.Len(Me.m_objMenu.MenuBarCssClass) > 0) Then
                    str = (str & Me.m_objMenu.MenuBarCssClass)
                End If
            ElseIf (Strings.Len(Me.m_objMenu.MenuCssClass) > 0) Then
                str = (str & Me.m_objMenu.MenuCssClass)
            End If
            writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
            writer.AddAttribute(HtmlTextWriterAttribute.Class, str)
            writer.AddAttribute(HtmlTextWriterAttribute.Name, Me.m_objMenu.UniqueID)
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Me.m_objMenu.ClientID)
            writer.RenderBeginTag(HtmlTextWriterTag.Div)
            Me.RenderChildren(writer)
            writer.RenderEndTag
        End Sub

        Public Sub RenderMenu(ByVal writer As HtmlTextWriter, ByVal Menu As DNNMenu) Implements IDNNMenuWriter.RenderMenu
            Me.m_objMenu = Menu
            Me.RenderControl(writer)
        End Sub


        ' Properties
        Public Property ForceFullMenu As Boolean
            Get
                Return Me.m_blnForceFullMenu
            End Get
            Set(ByVal Value As Boolean)
                Me.m_blnForceFullMenu = Value
            End Set
        End Property


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private m_objMenu As DNNMenu
        Private m_blnForceFullMenu As Boolean
    End Class
End Namespace

