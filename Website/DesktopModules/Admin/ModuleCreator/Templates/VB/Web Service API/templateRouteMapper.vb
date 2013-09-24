#Region "Copyright"

' 
' Copyright (c) [YEAR]
' by [OWNER]
' 

#End Region

#Region "Using Statements"

Imports DotNetNuke.Web.Api

#End Region

Namespace [OWNER].[MODULE]
    Public Class [MODULE]RouteMapper
        Implements IServiceRouteMapper
        Public Sub RegisterRoutes(mapRouteManager As IMapRoute)
            mapRouteManager.MapHttpRoute("[OWNER].[MODULE]", "default", "{controller}/{action}", New () {"[OWNER].[MODULE]"})
        End Sub
    End Class
End Namespace
