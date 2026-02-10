Imports System
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    Friend Interface IMenuNodeWriter
        ' Methods
        Sub RenderNode(ByVal writer As HtmlTextWriter, ByVal Node As MenuNode)
    End Interface
End Namespace

