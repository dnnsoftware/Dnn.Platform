' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.WebControls
Imports System
Imports System.Web.UI.Design
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.Design.WebControls
    Public Class DNNTabStripDesigner
        Inherits ControlDesigner
        ' Methods
        Public Overrides Function GetDesignTimeHtml() As String
            Dim designTimeHtml As String
            Dim component As DNNTabStrip = DirectCast(MyBase.Component, DNNTabStrip)
            Dim tabs As TabStripTabCollection = component.Tabs
            Dim forceDownLevel As Boolean = component.ForceDownLevel
            component.ForceDownLevel = True
            If (tabs.Count <> 0) Then
                designTimeHtml = MyBase.GetDesignTimeHtml
            Else
                tabs.Add(New DNNTab("Tab 1"))
                tabs.Add(New DNNTab("Tab 2"))
                Dim child As New Panel
                Dim unit2 As New Unit("100px")
                child.Width = unit2
                unit2 = New Unit("50px")
                child.Height = unit2
                tabs(0).Controls.Add(child)
                component.SelectedIndex = 0
                designTimeHtml = MyBase.GetDesignTimeHtml
                tabs.Clear
            End If
            component.ForceDownLevel = forceDownLevel
            Return designTimeHtml
        End Function

    End Class
End Namespace

