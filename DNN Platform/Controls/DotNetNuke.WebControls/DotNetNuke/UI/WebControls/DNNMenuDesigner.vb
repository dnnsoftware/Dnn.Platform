' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.IO
Imports System.Web.UI
Imports System.Web.UI.Design
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    Public Class DNNMenuDesigner
        Inherits ControlDesigner
        ' Methods
        Public Overrides Function GetDesignTimeHtml() As String
            Dim str As String
            Dim component As DNNMenu = DirectCast(Me.Component, DNNMenu)
            If (component.ID.Length <= 0) Then
                str = Nothing
            Else
                Dim unit2 As Unit
                Dim writer As New StringWriter
                Dim writer2 As New HtmlTextWriter(writer)
                Dim label As New Label With { _
                    .CssClass = (component.MenuBarCssClass & " " & component.DefaultNodeCssClass), _
                    .Text = component.ID _
                }
                If (component.Orientation = Orientation.Horizontal) Then
                    unit2 = New Unit("100%")
                    label.Width = unit2
                Else
                    unit2 = New Unit(500)
                    label.Height = unit2
                End If
                label.RenderControl(writer2)
                str = writer.ToString
            End If
            Return str
        End Function


        ' Properties
        Public Overrides ReadOnly Property AllowResize As Boolean
            Get
                Return False
            End Get
        End Property

    End Class
End Namespace

