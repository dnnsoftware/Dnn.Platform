' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Utilities
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class DNNTreeUpLevelWriter
        Inherits WebControl
        Implements IDNNTreeWriter
        ' Methods
        Public Sub New()
            DNNTreeUpLevelWriter.__ENCAddToList(Me)
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
            SyncLock DNNTreeUpLevelWriter.__ENCList
                If (DNNTreeUpLevelWriter.__ENCList.Count = DNNTreeUpLevelWriter.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNTreeUpLevelWriter.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNTreeUpLevelWriter.__ENCList.RemoveRange(index, (DNNTreeUpLevelWriter.__ENCList.Count - index))
                            DNNTreeUpLevelWriter.__ENCList.Capacity = DNNTreeUpLevelWriter.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNTreeUpLevelWriter.__ENCList(num2)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNTreeUpLevelWriter.__ENCList(index) = DNNTreeUpLevelWriter.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNTreeUpLevelWriter.__ENCList.Add(New WeakReference(value))
            End SyncLock
        End Sub

        Protected Function MarshalledProperties() As Hashtable Implements IDNNTreeWriter.MarshalledProperties
            Dim hashtable2 As New Hashtable
            Dim flag As Boolean = (Me._tree.ImageList.Count > 0)
            If flag Then
                Dim enumerator As IEnumerator = Nothing
                Dim expression As String = ""
                Try 
                    enumerator = Me._tree.ImageList.GetEnumerator
                    Do While True
                        flag = enumerator.MoveNext
                        If Not flag Then
                            Exit Do
                        End If
                        Dim current As NodeImage = DirectCast(enumerator.Current, NodeImage)
                        expression = (expression & Conversions.ToString(Interaction.IIf((Strings.Len(expression) > 0), ",", "")) & current.ImageUrl)
                    Loop
                Finally
                    If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                        TryCast(enumerator,IDisposable).Dispose
                    End If
                End Try
                hashtable2.Add("imagelist", expression)
            End If
            hashtable2.Add("postback", ClientAPI.GetPostBackEventReference(Me._tree, ("[NODEID]" & ClientAPI.COLUMN_DELIMITER & "Click")))
            If Me._tree.PopulateNodesFromClient Then
                If ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.XMLHTTP) Then
                    hashtable2.Add("callback", ClientAPI.GetCallbackEventReference(Me._tree, "'[NODEXML]'", "this.callBackSuccess", "tNode", "this.callBackFail", "this.callBackStatus"))
                Else
                    hashtable2.Add("callback", ClientAPI.GetPostBackClientHyperlink(Me._tree, ("[NODEID]" & ClientAPI.COLUMN_DELIMITER & "OnDemand")))
                End If
                If (Strings.Len(Me._tree.CallbackStatusFunction) > 0) Then
                    hashtable2.Add("callbackSF", Me._tree.CallbackStatusFunction)
                End If
            End If
            Return hashtable2
        End Function

        Protected Overrides Sub RenderChildren(ByVal writer As HtmlTextWriter)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me._tree.TreeNodes.GetEnumerator
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

        Protected Overrides Sub RenderContents(ByVal writer As HtmlTextWriter)
            writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "DNNTree")
            writer.AddAttribute(HtmlTextWriterAttribute.Name, Me._tree.UniqueID)
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Me._tree.ClientID)
            writer.RenderBeginTag(HtmlTextWriterTag.Div)
            writer.RenderEndTag
        End Sub

        Public Sub RenderTree(ByVal writer As HtmlTextWriter, ByVal tree As DnnTree) Implements IDNNTreeWriter.RenderTree
            Me._tree = tree
            Me.RenderControl(writer)
        End Sub


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private _tree As DnnTree
    End Class
End Namespace

