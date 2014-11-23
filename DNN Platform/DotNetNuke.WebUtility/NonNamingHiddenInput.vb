Imports System.Web.UI.HtmlControls

Namespace DotNetNuke.UI.Utilities
    'do not want this control for dnnVariable to ever use a naming container
    'somewhat of a hack here...
    Public Class NonNamingHiddenInput : Inherits HtmlInputHidden
        Private m_ValueSet As Boolean = False

        Public Overrides Property Value() As String
            Get
                If Len(Me.Page.Request.Form(Me.ID)) > 0 AndAlso m_ValueSet = False Then
                    Return Me.Page.Request.Form(Me.ID)
                End If
                Return MyBase.Value
            End Get
            Set(ByVal value As String)
                m_ValueSet = True
                MyBase.Value = value
            End Set
        End Property
        Public Overrides ReadOnly Property NamingContainer() As System.Web.UI.Control
            Get
                Return Nothing
            End Get
        End Property
    End Class
End Namespace