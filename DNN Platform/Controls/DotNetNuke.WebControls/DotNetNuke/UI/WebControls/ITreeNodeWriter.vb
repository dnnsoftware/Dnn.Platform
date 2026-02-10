Imports System
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    Friend Interface ITreeNodeWriter
        ' Methods
        Sub RenderNode(ByVal writer As HtmlTextWriter, ByVal Node As TreeNode)
    End Interface
End Namespace

