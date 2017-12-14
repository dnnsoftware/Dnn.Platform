'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2017
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'

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
        Public Sub RaisePostBackEvent(ByVal strEventArgument As String) Implements System.Web.UI.IPostBackEventHandler.RaisePostBackEvent
            Dim objArg As ClientAPIPostBackEventArgs = New ClientAPIPostBackEventArgs(strEventArgument)
            If Not EventHandlers(objArg.EventName) Is Nothing Then
                EventHandlers(objArg.EventName)(objArg)
            End If
        End Sub
    End Class

End Namespace
