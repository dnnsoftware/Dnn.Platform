' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Utilities
Imports DotNetNuke.UI.Utilities.Animation
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.ComponentModel
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    <TypeConverter(GetType(ExpandableObjectConverter))> _
    Public Class DNNAnimation
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
            If Not Object.ReferenceEquals(Me.m_State, Nothing) Then
                obj2 = DirectCast(Me.m_State, IStateManager).SaveViewState
            End If
            Return obj2
        End Function

        Protected Sub TrackViewState() Implements IStateManager.TrackViewState
            Me.m_Marked = True
        End Sub


        ' Properties
        <NotifyParentProperty(True), DefaultValue(0), ClientProperty, ClientPropertyName("anim")> _
        Public Property AnimationType As AnimationType
            Get
                Return If(Not ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.Motion), AnimationType.None, If((Strings.Len(Me.ViewState("AnimationType")) <> 0), DirectCast(Conversions.ToInteger(Me.ViewState("AnimationType")), AnimationType), AnimationType.None))
            End Get
            Set(ByVal value As AnimationType)
                Me.ViewState("AnimationType") = value
            End Set
        End Property

        <ClientPropertyName("easeDir"), DefaultValue(1), ClientProperty, NotifyParentProperty(True)> _
        Public Property EasingDirection As EasingDirection
            Get
                Return If((Strings.Len(Me.ViewState("EasingDirection")) <> 0), DirectCast(Conversions.ToInteger(Me.ViewState("EasingDirection")), EasingDirection), EasingDirection.In)
            End Get
            Set(ByVal value As EasingDirection)
                Me.ViewState("EasingDirection") = value
            End Set
        End Property

        <DefaultValue(3), NotifyParentProperty(True), ClientProperty, ClientPropertyName("easeType")> _
        Public Property EasingType As EasingType
            Get
                Return If((Strings.Len(Me.ViewState("EasingType")) <> 0), DirectCast(Conversions.ToInteger(Me.ViewState("EasingType")), EasingType), EasingType.Expo)
            End Get
            Set(ByVal value As EasingType)
                Me.ViewState("EasingType") = value
            End Set
        End Property

        <DefaultValue(1), NotifyParentProperty(True), ClientProperty, ClientPropertyName("animLen")> _
        Public Property Length As Integer
            Get
                Return If((Strings.Len(Me.ViewState("Length")) <> 0), Conversions.ToInteger(Me.ViewState("Length")), 1)
            End Get
            Set(ByVal value As Integer)
                Me.ViewState("Length") = value
            End Set
        End Property

        <ClientProperty, NotifyParentProperty(True), DefaultValue(10), ClientPropertyName("animInt")> _
        Public Property Interval As Integer
            Get
                Return If((Strings.Len(Me.ViewState("Interval")) <> 0), Conversions.ToInteger(Me.ViewState("Interval")), 10)
            End Get
            Set(ByVal value As Integer)
                Me.ViewState("Interval") = value
            End Set
        End Property

        Protected ReadOnly Property ViewState As StateBag
            Get
                If Object.ReferenceEquals(Me.m_State, Nothing) Then
                    Me.m_State = New StateBag(True)
                    If Me.IsTrackingViewState Then
                        DirectCast(Me.m_State, IStateManager).TrackViewState
                    End If
                End If
                Return Me.m_State
            End Get
        End Property

        Protected ReadOnly Property IsTrackingViewState As Boolean Implements System.Web.UI.IStateManager.IsTrackingViewState
            Get
                Return Me.m_Marked
            End Get
        End Property


        ' Fields
        Private m_Marked As Boolean
        Private m_State As StateBag
    End Class
End Namespace

