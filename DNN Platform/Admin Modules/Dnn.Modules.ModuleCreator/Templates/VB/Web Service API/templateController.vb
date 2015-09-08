#Region "Copyright"

' 
' Copyright (c) _YEAR_
' by _OWNER_
' 

#End Region

#Region "Using Statements"

Imports System
Imports System.Net
Imports System.Net.Http
Imports System.Web.Http
Imports DotNetNuke.Web.Api

#End Region

Namespace _OWNER_._MODULE_
    <AllowAnonymous> _
    Public Class _MODULE_Controller
        Inherits DnnApiController
        <HttpGet> _
        Public Function MyResponse() As HttpResponseMessage
            Return Request.CreateResponse(HttpStatusCode.OK, "Hello World!")
        End Function
    End Class
End Namespace
