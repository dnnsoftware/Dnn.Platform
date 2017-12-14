#Region "Copyright"

' 
' Copyright (c) _YEAR_
' by _OWNER_
' 

#End Region

#Region "Using Statements"

Imports DotNetNuke.Web.Api

#End Region

Namespace _OWNER_._MODULE_
    Public Class _MODULE_RouteMapper
        Implements IServiceRouteMapper
        Public Sub RegisterRoutes(mapRouteManager As IMapRoute)
            mapRouteManager.MapHttpRoute("_OWNER_._MODULE_", "default", "{controller}/{action}", New () {"_OWNER_._MODULE_"})
        End Sub
    End Class
End Namespace
