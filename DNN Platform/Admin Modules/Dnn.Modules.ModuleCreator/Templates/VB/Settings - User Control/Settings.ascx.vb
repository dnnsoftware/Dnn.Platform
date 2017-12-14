#Region "Copyright"

' 
' Copyright (c) _YEAR_
' by _OWNER_
' 

#End Region

#Region "Using Statements"

Imports System
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Services.Exceptions

#End Region

Namespace _OWNER_._MODULE_

    Partial Public Class Settings
        Inherits ModuleSettingsBase

#Region "Base Method Implementations"

        Public Overrides Sub LoadSettings()
            Try
                If Not Page.IsPostBack Then
                    txtField.Text = DirectCast(TabModuleSettings("field"), String)
                End If
            Catch exc As Exception
                ' Module failed to load
                Exceptions.ProcessModuleLoadException(Me, exc)
            End Try
        End Sub

        Public Overrides Sub UpdateSettings()
            Try
                ModuleController.Instance.UpdateTabModuleSetting(TabModuleId, "field", txtField.Text)
            Catch exc As Exception
                ' Module failed to load
                Exceptions.ProcessModuleLoadException(Me, exc)
            End Try
        End Sub

#End Region

    End Class

End Namespace
