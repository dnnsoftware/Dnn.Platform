' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.WebControls
Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.Web.UI

Namespace DotNetNuke.UI.Design.WebControls
    Public Class DNNToolBarButtonActionTypeConverter
        Inherits TypeConverter
        ' Methods
        Private Function AttachedControl(ByVal context As ITypeDescriptorContext) As Control
            If Object.ReferenceEquals(Me.m_objAttached, Nothing) Then
                Dim parent As DNNToolBar = DirectCast(DirectCast(context.Instance, DNNToolBarButton).Parent, DNNToolBar)
                Me.m_objAttached = parent.AttachedControl
            End If
            Return Me.m_objAttached
        End Function

        Public Overrides Function GetStandardValues(ByVal context As ITypeDescriptorContext) As StandardValuesCollection
            Dim values As New Collection
            Dim commonActions As String() = DirectCast(context.Instance, DNNToolBarButton).CommonActions
            Dim index As Integer = 0
            Do While True
                Dim flag As Boolean = (index < commonActions.Length)
                If Not flag Then
                    If Me.SupportsActions(context) Then
                        Dim actions As String() = DirectCast(Me.AttachedControl(context), IDNNToolBarSupportedActions).Actions
                        Dim num2 As Integer = 0
                        Do While True
                            flag = (num2 < actions.Length)
                            If Not flag Then
                                Exit Do
                            End If
                            Dim str2 As String = actions(num2)
                            values.Add(str2, Nothing, Nothing, Nothing)
                            num2 += 1
                        Loop
                    End If
                    Return New StandardValuesCollection(values)
                End If
                Dim item As String = commonActions(index)
                values.Add(item, Nothing, Nothing, Nothing)
                index += 1
            Loop

            Return Nothing
        End Function

        Public Overrides Function GetStandardValuesExclusive(ByVal context As ITypeDescriptorContext) As Boolean
            Return False
        End Function

        Public Overrides Function GetStandardValuesSupported(ByVal context As ITypeDescriptorContext) As Boolean
            Return Me.SupportsActions(context)
        End Function

        Private Function SupportsActions(ByVal context As ITypeDescriptorContext) As Boolean
            Return ((Not Me.AttachedControl(context) Is Nothing) AndAlso TypeOf Me.AttachedControl(context) Is IDNNToolBarSupportedActions)
        End Function


        ' Fields
        Private m_objAttached As Control
    End Class
End Namespace

