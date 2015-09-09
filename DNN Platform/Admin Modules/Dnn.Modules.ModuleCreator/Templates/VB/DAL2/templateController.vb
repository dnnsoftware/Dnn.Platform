#Region "Copyright"

' 
' Copyright (c) _YEAR_
' by _OWNER_
' 

#End Region

#Region "Using Statements"

Imports System.Collections.Generic
Imports DotNetNuke.Data

#End Region

Namespace _OWNER_._MODULE_
    Public Class _MODULE_Controller
        Public Sub Add_MODULE_(_MODULE_ As _MODULE_Info)
            Using ctx As IDataContext = DataContext.Instance()
                Dim rep = ctx.GetRepository(Of _MODULE_Info)()
                rep.Insert(_MODULE_)
            End Using
        End Sub

        Public Sub Delete_MODULE_(_MODULE_ As _MODULE_Info)
            Using ctx As IDataContext = DataContext.Instance()
                Dim rep = ctx.GetRepository(Of _MODULE_Info)()
                rep.Delete(_MODULE_)
            End Using
        End Sub

        Public Function Get_MODULE_s(moduleId As Integer) As IEnumerable(Of _MODULE_Info)
            Dim _MODULE_s As IEnumerable(Of _MODULE_Info)

            Using ctx As IDataContext = DataContext.Instance()
                Dim rep = ctx.GetRepository(Of _MODULE_Info)()
                _MODULE_s = rep.[Get](moduleId)
            End Using
            Return _MODULE_s
        End Function

        Public Function Get_MODULE_(_MODULE_Id As Integer, moduleId As Integer) As _MODULE_Info
            Dim _MODULE_ As _MODULE_Info

            Using ctx As IDataContext = DataContext.Instance()
                Dim rep = ctx.GetRepository(Of _MODULE_Info)()
                _MODULE_ = rep.GetById(_MODULE_Id, moduleId)
            End Using
            Return _MODULE_
        End Function

        Public Sub Update_MODULE_(_MODULE_ As _MODULE_Info)
            Using ctx As IDataContext = DataContext.Instance()
                Dim rep = ctx.GetRepository(Of _MODULE_Info)()
                rep.Update(_MODULE_)
            End Using
        End Sub
    End Class
End Namespace
