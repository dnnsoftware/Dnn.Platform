Imports System
Imports System.Collections
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    Friend Interface IDNNTreeWriter
        ' Methods
        Function MarshalledProperties() As Hashtable
        Sub RenderTree(ByVal writer As HtmlTextWriter, ByVal tree As DnnTree)
    End Interface
End Namespace

