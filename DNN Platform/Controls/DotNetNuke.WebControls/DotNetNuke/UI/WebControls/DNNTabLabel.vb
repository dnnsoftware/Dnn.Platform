' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.ComponentModel
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    Public Class DNNTabLabel
        Implements IStateManager, IUrlResolutionService
        ' Methods
        Protected Sub LoadViewState(ByVal state As Object) Implements IStateManager.LoadViewState
            If (Not state Is Nothing) Then
                DirectCast(Me.ViewState, IStateManager).LoadViewState(state)
            End If
        End Sub

        Public Function ResolveClientUrl(ByVal Url As String) As String Implements IUrlResolutionService.ResolveClientUrl
            Return Url
        End Function

        Protected Function SaveViewState() As Object Implements IStateManager.SaveViewState
            Dim obj2 As Object = Nothing
            If Not Object.ReferenceEquals(Me.m_oState, Nothing) Then
                obj2 = DirectCast(Me.m_oState, IStateManager).SaveViewState
            End If
            Return obj2
        End Function

        Protected Sub TrackViewState() Implements IStateManager.TrackViewState
            Me.m_bMarked = True
        End Sub


        ' Properties
        <Localizable(True), Category("Appearance"), NotifyParentProperty(True), DefaultValue("")> _
        Public Overridable Property [Text] As String
            Get
                Return Conversions.ToString(Me.ViewState("Text"))
            End Get
            Set(ByVal value As String)
                Me.ViewState("Text") = value
            End Set
        End Property

        <ClientPropertyName("csssel"), ClientProperty> _
        Public Property CssClassSelected As String
            Get
                Return Conversions.ToString(Me.ViewState("CssClassSelected"))
            End Get
            Set(ByVal value As String)
                Me.ViewState("CssClassSelected") = value
            End Set
        End Property

        <ClientProperty, ClientPropertyName("csshover")> _
        Public Property CssClassHover As String
            Get
                Return Conversions.ToString(Me.ViewState("CssClassHover"))
            End Get
            Set(ByVal value As String)
                Me.ViewState("CssClassHover") = value
            End Set
        End Property

        <ClientPropertyName("css"), ClientProperty> _
        Public Property CssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("CssClass"))
            End Get
            Set(ByVal value As String)
                Me.ViewState("CssClass") = value
            End Set
        End Property

        <ClientProperty, ClientPropertyName("cssdisabled")> _
        Public Property CssClassDisabled As String
            Get
                Return Conversions.ToString(Me.ViewState("CssClassDisabled"))
            End Get
            Set(ByVal value As String)
                Me.ViewState("CssClassDisabled") = value
            End Set
        End Property

        Public Property ImageUrl As String
            Get
                Return Conversions.ToString(Me.ViewState("ImageUrl"))
            End Get
            Set(ByVal value As String)
                Me.ViewState("ImageUrl") = value
            End Set
        End Property

        Protected ReadOnly Property ViewState As StateBag
            Get
                If Object.ReferenceEquals(Me.m_oState, Nothing) Then
                    Me.m_oState = New StateBag(True)
                    If Me.IsTrackingViewState Then
                        DirectCast(Me.m_oState, IStateManager).TrackViewState
                    End If
                End If
                Return Me.m_oState
            End Get
        End Property

        Protected ReadOnly Property IsTrackingViewState As Boolean Implements IStateManager.IsTrackingViewState
            Get
                Return Me.m_bMarked
            End Get
        End Property


        ' Fields
        Private m_bMarked As Boolean
        Private m_oState As StateBag
    End Class
End Namespace

