#Region "Copyright"

' 
' Copyright (c) _YEAR_
' by _OWNER_
' 

#End Region

Imports System
Imports DotNetNuke

Namespace _OWNER_._MODULE_

    Public Class _MODULE_Job

        Inherits DotNetNuke.Services.Scheduling.SchedulerClient

        Public Sub New(ByVal objScheduleHistoryItem As DotNetNuke.Services.Scheduling.ScheduleHistoryItem)
            MyBase.New()
            Me.ScheduleHistoryItem = objScheduleHistoryItem
        End Sub

        Public Overrides Sub DoWork()
            Try
                Me.Progressing()
                Dim strMessage As String = Processing()
                Me.ScheduleHistoryItem.Succeeded = True
                Me.ScheduleHistoryItem.AddLogNote("_MODULE_ Succeeded")
            Catch exc As Exception    
                Me.ScheduleHistoryItem.Succeeded = False
                Me.ScheduleHistoryItem.AddLogNote("_MODULE_ Failed")
                Me.Errored(exc)
            End Try
        End Sub

        Public Function Processing() As String
            Dim Message As String = ""
            Return Message 
        End Function

    End Class

End Namespace

