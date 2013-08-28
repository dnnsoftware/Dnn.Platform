#Region "Copyright"

' 
' Copyright (c) [YEAR]
' by [OWNER]
' 

#End Region

#Region "Using Statements"

Imports System.Collections.Generic
Imports DotNetNuke.Data

#End Region

Namespace [OWNER].[MODULE]
    Public Class [MODULE]Controller
        Public Sub Add[MODULE]([MODULE] As [MODULE]Info)
            Using ctx As IDataContext = DataContext.Instance()
                Dim rep = ctx.GetRepository(Of [MODULE]Info)()
                rep.Insert([MODULE])
            End Using
        End Sub

        Public Sub Delete[MODULE]([MODULE] As [MODULE]Info)
            Using ctx As IDataContext = DataContext.Instance()
                Dim rep = ctx.GetRepository(Of [MODULE]Info)()
                rep.Delete([MODULE])
            End Using
        End Sub

        Public Function Get[MODULE]s(moduleId As Integer) As IEnumerable(Of [MODULE]Info)
            Dim [MODULE]s As IEnumerable(Of [MODULE]Info)

            Using ctx As IDataContext = DataContext.Instance()
                Dim rep = ctx.GetRepository(Of [MODULE]Info)()
                [MODULE]s = rep.[Get](moduleId)
            End Using
            Return [MODULE]s
        End Function

        Public Function Get[MODULE]([MODULE]Id As Integer, moduleId As Integer) As [MODULE]Info
            Dim [MODULE] As [MODULE]Info

            Using ctx As IDataContext = DataContext.Instance()
                Dim rep = ctx.GetRepository(Of [MODULE]Info)()
                [MODULE] = rep.GetById([MODULE]Id, moduleId)
            End Using
            Return [MODULE]
        End Function

        Public Sub Update[MODULE]([MODULE] As [MODULE]Info)
            Using ctx As IDataContext = DataContext.Instance()
                Dim rep = ctx.GetRepository(Of [MODULE]Info)()
                rep.Update([MODULE])
            End Using
        End Sub
    End Class
End Namespace
