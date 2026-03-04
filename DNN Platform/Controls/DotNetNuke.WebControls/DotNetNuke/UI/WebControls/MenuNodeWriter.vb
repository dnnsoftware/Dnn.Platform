' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Utilities
Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class MenuNodeWriter
        Implements IMenuNodeWriter
        ' Methods
        Public Sub New()
        End Sub

        Public Sub New(ByVal blnForceFullMenu As Boolean)
            Me.ForceFullMenu = blnForceFullMenu
        End Sub

        Public Sub New(ByVal blnForceFullMenu As Boolean, ByVal eOrientation As Orientation)
            Me.ForceFullMenu = blnForceFullMenu
            Me.Orientation = eOrientation
        End Sub

        Private Function GetNodeCss(ByVal oNode As MenuNode) As String
            Dim cssClass As String = oNode.DNNMenu.CssClass
            If (oNode.Level > 0) Then
                cssClass = oNode.DNNMenu.DefaultChildNodeCssClass
            End If
            If (Strings.Len(oNode.CSSClass) > 0) Then
                cssClass = oNode.CSSClass
            End If
            If oNode.Selected Then
                cssClass = If((Strings.Len(oNode.CSSClassSelected) <= 0), (cssClass & " " & oNode.DNNMenu.DefaultNodeCssClassSelected), (cssClass & " " & oNode.CSSClassSelected))
            End If
            Return cssClass
        End Function

        Protected Sub Render(ByVal writer As HtmlTextWriter)
            Dim node As MenuNode = Nothing
            Dim iD As String = Nothing
            If (Me.m_Node.DNNMenu.SelectedMenuNodes.Count > 0) Then
                node = DirectCast(Me.m_Node.DNNMenu.SelectedMenuNodes(Me.m_Node.DNNMenu.SelectedMenuNodes.Count), MenuNode)
            End If
            If Not Object.ReferenceEquals(Me.m_Node.Parent, Nothing) Then
                iD = Me.m_Node.Parent.ID
            End If
            If (((node Is Nothing) OrElse (Me.m_Node.Selected OrElse ((node.ID = Me.m_Node.ID) OrElse ((node.ID = iD) OrElse ((Not node.Parent Is Nothing) AndAlso (node.Parent.ID = iD)))))) OrElse Me.ForceFullMenu) Then
                If (If(((node Is Nothing) OrElse ((node.ID <> Me.m_Node.ID) OrElse (String.IsNullOrEmpty(iD) OrElse (iD = Me.m_Node.DNNMenu.ClientID)))), 0, 1) <> 0) Then
                    Me.RenderParentLink(writer, iD)
                End If
                Me.RenderContents(writer)
            End If
            If (If((Not Me.m_Node.HasNodes OrElse (Not Me.m_Node.Selected AndAlso Not Me.ForceFullMenu)), 0, 1) = 0) Then
                If (If(((node Is Nothing) OrElse (node.Level <= Me.m_Node.Level)), 0, 1) <> 0) Then
                    Me.RenderChildren(writer)
                End If
            Else
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "Child")
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
                If (Me.ForceFullMenu OrElse (Me.Orientation = Orientation.Vertical)) Then
                    writer.RenderBeginTag(HtmlTextWriterTag.Div)
                Else
                    writer.RenderBeginTag(HtmlTextWriterTag.Span)
                End If
                Me.RenderChildren(writer)
                writer.RenderEndTag
            End If
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

        Protected Sub RenderContents(ByVal writer As HtmlTextWriter)
            Me.RenderOpenTag(writer)
            Me.RenderNodeIcon(writer)
            Me.RenderNodeText(writer)
            Me.RenderNodeArrow(writer)
            writer.RenderEndTag
        End Sub

        Public Sub RenderNode(ByVal writer As HtmlTextWriter, ByVal Node As MenuNode) Implements IMenuNodeWriter.RenderNode
            Me.m_Node = Node
            Me.Render(writer)
        End Sub

        Protected Sub RenderNodeArrow(ByVal writer As HtmlTextWriter)
            If Me.m_Node.HasNodes Then
                Dim label As New Label
                label.RenderBeginTag(writer)
                If (Me.m_Node.Level = 0) Then
                    If (Strings.Len(Me.m_Node.DNNMenu.RootArrowImage) > 0) Then
                        Dim image As New Image() With {
                            .ImageUrl = Me.m_Node.DNNMenu.RootArrowImage
                        }
                        image.RenderControl(writer)
                    End If
                ElseIf (Strings.Len(Me.m_Node.DNNMenu.ChildArrowImage) > 0) Then
                    Dim image As New Image() With {
                        .ImageUrl = Me.m_Node.DNNMenu.ChildArrowImage
                    }
                    image.RenderControl(writer)
                End If
                label.RenderEndTag(writer)
            End If
        End Sub

        Protected Sub RenderNodeIcon(ByVal writer As HtmlTextWriter)
            Dim label As New Label
            If (Strings.Len(Me.m_Node.CSSIcon) > 0) Then
                label.CssClass = Me.m_Node.CSSIcon
            ElseIf (Strings.Len(Me.m_Node.DNNMenu.DefaultIconCssClass) > 0) Then
                label.CssClass = Me.m_Node.DNNMenu.DefaultIconCssClass
            End If
            label.RenderBeginTag(writer)
            If (Me.m_Node.ImageIndex > -1) Then
                Dim objA As NodeImage = Me.m_Node.DNNMenu.ImageList(Me.m_Node.ImageIndex)
                If Not Object.ReferenceEquals(objA, Nothing) Then
                    Dim image As New Image() With { _
                        .ImageUrl = objA.ImageUrl _
                    }
                    image.RenderControl(writer)
                    writer.Write("&nbsp;")
                End If
            End If
            label.RenderEndTag(writer)
        End Sub

        Protected Sub RenderNodeText(ByVal writer As HtmlTextWriter)
            Dim link As New HyperLink
            Dim dNNMenu As DNNMenu = Me.m_Node.DNNMenu
            Dim str As String = WebControls.GetNodeJS(dNNMenu, Me.m_Node, dNNMenu.JSFunction, dNNMenu.Target, True)
            link.Text = Me.m_Node.Text
            link.NavigateUrl = If(Not Me.m_Node.Enabled, ClientAPI.GetPostBackClientHyperlink(dNNMenu, (Me.m_Node.ID & ClientAPI.COLUMN_DELIMITER & "Click")), If(Not String.IsNullOrEmpty(str), ("javascript:" & str), Me.m_Node.NavigateURL))
            If (Strings.Len(Me.m_Node.ToolTip) > 0) Then
                link.ToolTip = Me.m_Node.ToolTip
            End If
            Dim nodeCss As String = Me.GetNodeCss(Me.m_Node)
            If (Strings.Len(nodeCss) > 0) Then
                link.CssClass = nodeCss
            End If
            link.RenderControl(writer)
        End Sub

        Protected Sub RenderOpenTag(ByVal writer As HtmlTextWriter)
            writer.AddAttribute(HtmlTextWriterAttribute.Name, Me.m_Node.ID)
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Me.m_Node.ID)
            If (If((Me.ForceFullMenu OrElse (Me.Orientation = Orientation.Vertical)), 1, 0) = 0) Then
                writer.RenderBeginTag(HtmlTextWriterTag.Span)
            Else
                writer.AddStyleAttribute("padding-left", ((Me.m_Node.Level * 20).ToString(CultureInfo.InvariantCulture) & "px"))
                writer.RenderBeginTag(HtmlTextWriterTag.Div)
            End If
        End Sub

        Protected Sub RenderParentLink(ByVal writer As HtmlTextWriter, ByVal ID As String)
            Dim hyperlink As New HyperLink() With { _
                .Text = "..\", _
                .NavigateUrl = ClientAPI.GetPostBackClientHyperlink(Me.m_Node.DNNMenu, (ID & ClientAPI.COLUMN_DELIMITER & "Click")) _
            }
            hyperlink.RenderControl(writer)
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
        Private m_blnForceFullMenu As Boolean
        Private m_eOrientation As Orientation
    End Class
End Namespace

