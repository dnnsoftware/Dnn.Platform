' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System

Namespace DotNetNuke.UI.WebControls
    <AttributeUsage(AttributeTargets.Property)>
    Public Class ClientPropertyNameAttribute
        Inherits Attribute
        ' Methods
        Public Sub New(ByVal name As String)
            Me.m_Name = name
        End Sub


        ' Properties
        Public ReadOnly Property Name As String
            Get
                Return Me.m_Name
            End Get
        End Property


        ' Fields
        Private m_Name As String = ""
    End Class
End Namespace

