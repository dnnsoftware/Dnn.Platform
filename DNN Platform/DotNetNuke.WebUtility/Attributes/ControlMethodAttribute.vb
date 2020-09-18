Imports System

Namespace DotNetNuke.UI.Utilities
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)> _
    Public NotInheritable Class ControlMethodAttribute : Inherits Attribute
        Public Sub New()
        End Sub
    End Class

End Namespace

