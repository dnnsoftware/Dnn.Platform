' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class MenuNodeListWriter
        Implements IMenuNodeWriter
        ' Methods
        Public Sub New()
        End Sub

        Public Sub New(ByVal eOrientation As Orientation)
            Me.Orientation = eOrientation
        End Sub

        Private Function GetChildControlID(ByVal NodeID As String, ByVal Prefix As String) As String
            Return $"{Me.m_Node.DNNMenu.ClientID}{Prefix}{NodeID}"
        End Function

        Private Function GetLink() As HyperLink
            Dim link2 As New HyperLink
            If (If((Not Me.m_Node.Enabled OrElse (Me.m_Node.ClickAction <> eClickAction.Navigate)), 0, 1) <> 0) Then
                link2.NavigateUrl = Me.m_Node.NavigateURL
            End If
            Return link2
        End Function

        Protected Sub Render(ByVal writer As HtmlTextWriter)
            Me.RenderOpenTag(writer)
            Dim parent As HyperLink = Me.GetLink
            Me.RenderNodeIcon(parent)
            Me.RenderNodeText(parent)
            Me.RenderNodeArrow(parent)
            parent.RenderControl(writer)
            If Me.m_Node.HasNodes Then
                writer.AddAttribute(HtmlTextWriterAttribute.Id, Me.GetChildControlID(Me.m_Node.ID, "sub"))
                writer.RenderBeginTag(HtmlTextWriterTag.Ul)
                Me.RenderChildren(writer)
                writer.RenderEndTag
            End If
            writer.RenderEndTag
        End Sub

        Protected Sub RenderChildren(ByVal writer As HtmlTextWriter)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.m_Node.MenuNodes.GetEnumerator
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

        Public Sub RenderNode(ByVal writer As HtmlTextWriter, ByVal Node As MenuNode) Implements IMenuNodeWriter.RenderNode
            Me.m_Node = Node
            Me.Render(writer)
        End Sub

        Protected Sub RenderNodeArrow(ByVal parent As Control)
            If Me.m_Node.HasNodes Then
                Dim objA As Image = Nothing
                If (Me.m_Node.Level = 0) Then
                    If (Strings.Len(Me.m_Node.DNNMenu.RootArrowImage) > 0) Then
                        objA = New Image With { _
                            .ImageUrl = Me.m_Node.DNNMenu.RootArrowImage _
                        }
                    End If
                ElseIf (Strings.Len(Me.m_Node.DNNMenu.ChildArrowImage) > 0) Then
                    objA = New Image With { _
                        .ImageUrl = Me.m_Node.DNNMenu.ChildArrowImage _
                    }
                End If
                If Not Object.ReferenceEquals(objA, Nothing) Then
                    parent.Controls.Add(objA)
                End If
            End If
        End Sub

        Protected Sub RenderNodeIcon(ByVal parent As Control)
            Dim cSSIcon As String = ""
            If (Strings.Len(Me.m_Node.CSSIcon) > 0) Then
                cSSIcon = Me.m_Node.CSSIcon
            ElseIf (Strings.Len(Me.m_Node.DNNMenu.DefaultIconCssClass) > 0) Then
                cSSIcon = Me.m_Node.DNNMenu.DefaultIconCssClass
            End If
            If ((Me.m_Node.ImageIndex > -1) AndAlso (Me.m_Node.ImageIndex > -1)) Then
                Dim objA As NodeImage = Me.m_Node.DNNMenu.ImageList(Me.m_Node.ImageIndex)
                If Not Object.ReferenceEquals(objA, Nothing) Then
                    Dim child As New Image With { _
                        .ImageUrl = objA.ImageUrl, _
                        .AlternateText = " ", _
                        .CssClass = cSSIcon _
                    }
                    parent.Controls.Add(child)
                End If
            End If
        End Sub

        Protected Sub RenderNodeText(ByVal parent As Control)
            Dim child As New Label With { _
                .Text = Me.m_Node.Text _
            }
            If (Strings.Len(Me.m_Node.ToolTip) > 0) Then
                child.ToolTip = Me.m_Node.ToolTip
            End If
            parent.Controls.Add(child)
        End Sub

        Protected Sub RenderOpenTag(ByVal writer As HtmlTextWriter)
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Me.GetChildControlID(Me.m_Node.ID, "ctr"))
            writer.RenderBeginTag(HtmlTextWriterTag.Li)
        End Sub


        ' Properties
        Public Property Orientation As Orientation
            Get
                Return Me.m_eOrientation
            End Get
            Set(ByVal Value As Orientation)
                Me.m_eOrientation = Value
            End Set
        End Property


        ' Fields
        Private m_Node As MenuNode
        Private m_eOrientation As Orientation
    End Class
End Namespace

