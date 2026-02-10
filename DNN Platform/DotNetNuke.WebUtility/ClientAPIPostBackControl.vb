' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System.Diagnostics.CodeAnalysis
Imports System.Web.UI

Namespace DotNetNuke.UI.Utilities

    ''' -----------------------------------------------------------------------------
    ''' Project	 : DotNetNuke
    ''' Class	 : ClientAPIPostBackControl
    ''' 
    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Control used to register post-back events
    ''' </summary>
    ''' <remarks>
    ''' In order for a post-back event to be trapped we need to associate a control to 
    ''' handle the event.
    ''' </remarks>
    ''' <history>
    ''' 	[Jon Henning]	9/15/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Class ClientAPIPostBackControl : Inherits Control : Implements IPostBackEventHandler
        Delegate Sub PostBackEvent(ByVal Args As ClientAPIPostBackEventArgs)
        Private m_oEventHandlers As Hashtable = New Hashtable

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Property to access individual post back event handlers based off of event name
        ''' </summary>
        ''' <param name="strEventName">Event Name</param>
        ''' <returns>PostBackEvent</returns>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' 	[Jon Henning]	9/15/2004	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Public ReadOnly Property EventHandlers(ByVal strEventName As String) As PostBackEvent
            Get
                If m_oEventHandlers.Contains(strEventName) Then
                    Return CType(m_oEventHandlers(strEventName), PostBackEvent)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Adds a postback event handler to the control
        ''' </summary>
        ''' <param name="strEventName">Event Name</param>
        ''' <param name="objDelegate">Delegate for Function of type PostBackEvent</param>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' 	[Jon Henning]	9/15/2004	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Public Sub AddEventHandler(ByVal strEventName As String, ByVal objDelegate As PostBackEvent)
            If m_oEventHandlers.Contains(strEventName) = False Then
                m_oEventHandlers.Add(strEventName, objDelegate)
            Else
                m_oEventHandlers(strEventName) = CType(System.Delegate.Combine(CType(m_oEventHandlers(strEventName), ClientAPIPostBackControl.PostBackEvent), objDelegate), ClientAPIPostBackControl.PostBackEvent)
            End If
        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="objPage">Page</param>
        ''' <param name="strEventName">Event Name</param>
        ''' <param name="objDelegate">Delegate for Function of type PostBackEvent</param>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' 	[Jon Henning]	9/15/2004	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Public Sub New(ByVal objPage As Page, ByVal strEventName As String, ByVal objDelegate As PostBackEvent)
            ClientAPI.GetPostBackClientEvent(objPage, Me, "")
            AddEventHandler(strEventName, objDelegate)
        End Sub
        Public Sub New()

        End Sub
        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Function implementing IPostBackEventHandler which allows the ASP.NET page to invoke
        ''' the control's events
        ''' </summary>
        ''' <param name="strEventArgument"></param>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' 	[Jon Henning]	9/15/2004	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        <SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification := "Breaking change")>
        Public Sub RaisePostBackEvent(ByVal strEventArgument As String) Implements System.Web.UI.IPostBackEventHandler.RaisePostBackEvent
            Dim objArg As ClientAPIPostBackEventArgs = New ClientAPIPostBackEventArgs(strEventArgument)
            If Not EventHandlers(objArg.EventName) Is Nothing Then
                EventHandlers(objArg.EventName)(objArg)
            End If
        End Sub
    End Class

End Namespace
