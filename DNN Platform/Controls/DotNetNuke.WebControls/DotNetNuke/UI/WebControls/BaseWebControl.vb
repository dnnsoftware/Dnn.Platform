' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Utilities
Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    Public Class BaseWebControl
        Inherits WebControl
        ' Methods
        <DebuggerNonUserCode> _
        Public Sub New()
            BaseWebControl.__ENCAddToList(Me)
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
            SyncLock BaseWebControl.__ENCList
                If (BaseWebControl.__ENCList.Count = BaseWebControl.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (BaseWebControl.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            BaseWebControl.__ENCList.RemoveRange(index, (BaseWebControl.__ENCList.Count - index))
                            BaseWebControl.__ENCList.Capacity = BaseWebControl.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = BaseWebControl.__ENCList(num2)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                BaseWebControl.__ENCList(index) = BaseWebControl.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                BaseWebControl.__ENCList.Add(New WeakReference(value))
            End SyncLock
        End Sub

        Public Shared Sub RegisterInitialize(ByVal ctl As Control, ByVal func As String, ByVal propJson As String)
            Dim str As String = $"dnn.setVar('{ctl.ClientID}_p', '{ClientAPI.EscapeForJavascript(propJson)}');"
            ClientAPI.RegisterStartUpScript(ctl.Page, (ctl.ClientID & "_startup"), $"<script type=""text/javascript"">{str}dnn.controls.{func}($get('{ctl.ClientID}'));</script>")
        End Sub


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
    End Class
End Namespace

