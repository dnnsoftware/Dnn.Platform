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
    Friend NotInheritable Class TreeNodeWriter
        Implements ITreeNodeWriter
        ' Methods
        Private Function GetNodeCss(ByVal oNode As TreeNode) As String
            Dim cssClass As String = oNode.DNNTree.CssClass
            If (oNode.Level > 0) Then
                cssClass = oNode.DNNTree.DefaultChildNodeCssClass
            End If
            If (Strings.Len(oNode.CssClass) > 0) Then
                cssClass = oNode.CssClass
            End If
            If oNode.Selected Then
                cssClass = If((Strings.Len(oNode.CSSClassSelected) <= 0), (cssClass & " " & oNode.DNNTree.DefaultNodeCssClassSelected), (cssClass & " " & oNode.CSSClassSelected))
            End If
            Return cssClass
        End Function

        Protected Sub Render(ByVal writer As HtmlTextWriter)
            Me.RenderContents(writer)
            If (If((Not Me._Node.HasNodes OrElse (Not Me._Node.IsExpanded AndAlso Not Me._Node.DNNTree.IsCrawler)), 0, 1) <> 0) Then
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "Child")
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
                writer.RenderBeginTag(HtmlTextWriterTag.Div)
                Me.RenderChildren(writer)
                writer.RenderEndTag
            End If
        End Sub

        Protected Sub RenderChildren(ByVal writer As HtmlTextWriter)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me._Node.TreeNodes.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    DirectCast(enumerator.Current, TreeNode).Render(writer)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
        End Sub

        Protected Sub RenderContents(ByVal writer As HtmlTextWriter)
            Me.RenderOpenTag(writer)
            If (Me._Node.DNNTree.IndentWidth = 0) Then
                Me._Node.DNNTree.IndentWidth = 9
            End If
            If (Me._Node.Level > 0) Then
                Me.RenderSpacer(writer, (Me._Node.Level * Me._Node.DNNTree.IndentWidth))
            End If
            Me.RenderExpandNodeIcon(writer)
            Me.RenderNodeCheckbox(writer)
            Me.RenderNodeIcon(writer)
            Me.RenderNodeText(writer)
            writer.RenderEndTag
        End Sub

        Protected Sub RenderExpandNodeIcon(ByVal writer As HtmlTextWriter)
            If Not Me._Node.HasNodes Then
                Me.RenderSpacer(writer, Me._Node.DNNTree.ExpandCollapseImageWidth)
            Else
                Dim link As New HyperLink
                Dim image As New Image
                If (Me._Node.IsExpanded OrElse Me._Node.DNNTree.IsCrawler) Then
                    link.Text = TreeNodeWriter._expcol(1)
                    If (Strings.Len(Me._Node.DNNTree.ExpandedNodeImage) > 0) Then
                        image.ImageUrl = Me._Node.DNNTree.ExpandedNodeImage
                    End If
                Else
                    link.Text = TreeNodeWriter._expcol(0)
                    If (Strings.Len(Me._Node.DNNTree.CollapsedNodeImage) > 0) Then
                        image.ImageUrl = Me._Node.DNNTree.CollapsedNodeImage
                    End If
                End If
                link.NavigateUrl = ClientAPI.GetPostBackClientHyperlink(Me._Node.DNNTree, Me._Node.ID)
                If (Strings.Len(image.ImageUrl) <= 0) Then
                    link.RenderControl(writer)
                Else
                    link.RenderBeginTag(writer)
                    image.RenderControl(writer)
                    link.RenderEndTag(writer)
                End If
                image = Nothing
                link = Nothing
            End If
            writer.Write("&nbsp;")
        End Sub

        Public Sub RenderNode(ByVal writer As HtmlTextWriter, ByVal Node As TreeNode) Implements ITreeNodeWriter.RenderNode
            Me._Node = Node
            Me.Render(writer)
        End Sub

        Protected Sub RenderNodeCheckbox(ByVal writer As HtmlTextWriter)
            If Me._Node.DNNTree.CheckBoxes Then
                Dim box As New CheckBox With { _
                    .ID = (Me._Node.ID & TreeNode._separator & TreeNode._checkboxIDSufix), _
                    .Checked = Me._Node.Selected _
                }
                Dim expression As String = ""
                If (Strings.Len(Me._Node.JSFunction) > 0) Then
                    If Not Me._Node.JSFunction.EndsWith(";", StringComparison.Ordinal) Then
                        Dim node As TreeNode = Me._Node
                        node.JSFunction = (node.JSFunction & ";")
                    End If
                    expression = (expression & Me._Node.JSFunction)
                End If
                If (Strings.Len(Me._Node.DNNTree.JSFunction) > 0) Then
                    If Not Me._Node.DNNTree.JSFunction.EndsWith(";", StringComparison.Ordinal) Then
                        Dim dNNTree As DnnTree = Me._Node.DNNTree
                        dNNTree.JSFunction = (dNNTree.JSFunction & ";")
                    End If
                    expression = (expression & Me._Node.DNNTree.JSFunction)
                End If
                Dim str2 As String = (ClientAPI.GetPostBackClientHyperlink(Me._Node.DNNTree, (Me._Node.ID & ClientAPI.COLUMN_DELIMITER & "Click")).Replace("javascript:", "") & ";")
                Dim str As String = (ClientAPI.GetPostBackClientHyperlink(Me._Node.DNNTree, (Me._Node.ID & ClientAPI.COLUMN_DELIMITER & "Checked")).Replace("javascript:", "") & ";")
                expression = If(Me._Node.Selected, str, If((Strings.Len(expression) <= 0), (expression & str2), (("if (eval(""" & expression.Replace("""", """""") & """) != false) ") & str2 & " else " & str)))
                box.Attributes.Add("onclick", expression)
                box.RenderControl(writer)
                box = Nothing
                writer.Write("&nbsp;")
            End If
        End Sub

        Protected Sub RenderNodeIcon(ByVal writer As HtmlTextWriter)
            Dim label As New Label
            If (Strings.Len(Me._Node.CSSIcon) > 0) Then
                label.CssClass = Me._Node.CSSIcon
            ElseIf (Strings.Len(Me._Node.DNNTree.DefaultIconCssClass) > 0) Then
                label.CssClass = Me._Node.DNNTree.DefaultIconCssClass
            End If
            label.RenderBeginTag(writer)
            If (Me._Node.ImageIndex > -1) Then
                Dim objA As NodeImage = Me._Node.DNNTree.ImageList(Me._Node.ImageIndex)
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
            Dim expression As String = ""
            link.Text = Me._Node.Text
            If (Strings.Len(Me._Node.JSFunction) > 0) Then
                If Not Me._Node.JSFunction.EndsWith(";", StringComparison.Ordinal) Then
                    Dim node As TreeNode = Me._Node
                    node.JSFunction = (node.JSFunction & ";")
                End If
                expression = (expression & Me._Node.JSFunction)
            ElseIf (Strings.Len(Me._Node.DNNTree.JSFunction) > 0) Then
                If Not Me._Node.DNNTree.JSFunction.EndsWith(";", StringComparison.Ordinal) Then
                    Dim dNNTree As DnnTree = Me._Node.DNNTree
                    dNNTree.JSFunction = (dNNTree.JSFunction & ";")
                End If
                expression = (expression & Me._Node.DNNTree.JSFunction)
            End If
            If Me._Node.Enabled Then
                Select Case Me._Node.ClickAction
                    Case eClickAction.PostBack, eClickAction.Expand
                        If (Strings.Len(expression) > 0) Then
                            expression = ("if (eval(""" & expression.Replace("""", """""") & """) != false) ")
                        End If
                        expression = (expression & ClientAPI.GetPostBackClientHyperlink(Me._Node.DNNTree, (Me._Node.ID & ClientAPI.COLUMN_DELIMITER & "Click")).Replace("javascript:", ""))
                        Exit Select
                    Case eClickAction.Navigate
                        If (Strings.Len(expression) > 0) Then
                            expression = ("if (eval(""" & expression.Replace("""", """""") & """) != false) ")
                        End If
                        If (Strings.Len(Me._Node.DNNTree.Target) <= 0) Then
                            expression = (expression & "window.location.href='" & Me._Node.NavigateUrl & "';")
                        Else
                            Dim strArray As String() = New String() { expression, "window.frames.", Me._Node.DNNTree.Target, ".location.href='", Me._Node.NavigateUrl, "'; void(0);" }
                            expression = String.Concat(strArray)
                        End If
                        Exit Select
                    Case Else
                        Exit Select
                End Select
                link.NavigateUrl = ("javascript:" & expression)
            End If
            If (Strings.Len(Me._Node.ToolTip) > 0) Then
                link.ToolTip = Me._Node.ToolTip
            End If
            Dim nodeCss As String = Me.GetNodeCss(Me._Node)
            If (Strings.Len(nodeCss) > 0) Then
                link.CssClass = nodeCss
            End If
            link.RenderControl(writer)
        End Sub

        Protected Sub RenderOpenTag(ByVal writer As HtmlTextWriter)
            writer.AddAttribute(HtmlTextWriterAttribute.Name, Me._Node.ID)
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Me._Node.ID.Replace(TreeNode._separator, "_"))
            writer.RenderBeginTag(HtmlTextWriterTag.Div)
        End Sub

        Protected Sub RenderSpacer(ByVal writer As HtmlTextWriter, ByVal Width As Integer)
            writer.AddStyleAttribute("width", (Width.ToString(CultureInfo.InvariantCulture) & "px"))
            writer.AddStyleAttribute("height", "1px")
            writer.AddAttribute("src", (Me._Node.DNNTree.SystemImagesPath & "spacer.gif"))
            writer.RenderBeginTag(HtmlTextWriterTag.Img)
            writer.RenderEndTag
        End Sub


        ' Fields
        Private Shared ReadOnly _expcol As String() = New String() { "+", "-" }
        Private _Node As TreeNode
    End Class
End Namespace

