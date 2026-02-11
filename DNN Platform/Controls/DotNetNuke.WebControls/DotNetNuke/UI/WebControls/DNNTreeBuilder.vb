Imports System
Imports System.Collections
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    Public Class DNNTreeBuilder
        Inherits ControlBuilder
        ' Methods
        Public Overrides Function GetChildControlType(ByVal tagName As String, ByVal attribs As IDictionary) As Type
            Return If(Not tagName.EndsWith("TreeNode", StringComparison.OrdinalIgnoreCase), Nothing, GetType(TreeNode))
        End Function

    End Class
End Namespace

