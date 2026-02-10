Imports System
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    Friend Interface IDNNMenuWriter
        ' Methods
        Sub RenderMenu(ByVal writer As HtmlTextWriter, ByVal menu As DNNMenu)
    End Interface
End Namespace

