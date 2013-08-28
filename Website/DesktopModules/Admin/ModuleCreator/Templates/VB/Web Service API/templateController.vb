#Region "Copyright"

' 
' Copyright (c) [YEAR]
' by [OWNER]
' 

#End Region

#Region "Using Statements"

Imports System
Imports System.Net
Imports System.Net.Http
Imports System.Web.Http
Imports DotNetNuke.Web.Api

#End Region

Namespace [OWNER].[MODULE]
    <AllowAnonymous> _
    Public Class [MODULE]Controller
        Inherits DnnApiController
        <HttpGet> _
        Public Function MyResponse() As HttpResponseMessage
            Return Request.CreateResponse(HttpStatusCode.OK, "Hello World!")
        End Function
    End Class
End Namespace
