' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System.Globalization
Imports System.Web
Imports System.Web.UI

Namespace DotNetNuke.UI.Utilities

    Public Class ClientAPICallBackResponse
        Public Enum CallBackResponseStatusCode
            OK = 200
            GenericFailure = 400
            ControlNotFound = 404
            InterfaceNotSupported = 501
        End Enum

        Public Enum CallBackTypeCode
            Simple = 0
            ProcessPage = 1
            CallbackMethod = 2
            ProcessPageCallbackMethod = 3
            'XMLRPC
        End Enum

        Public Enum TransportTypeCode
            XMLHTTP = 0
            IFRAMEPost = 1
        End Enum

        Public Response As String = ""
        Public StatusCode As CallBackResponseStatusCode
        Public StatusDesc As String = ""
        Private m_objPage As Page
        Public CallBackType As CallBackTypeCode

        Public ReadOnly Property TransportType() As TransportTypeCode
            Get
                If Len(m_objPage.Request.Form("ctx")) > 0 Then
                    Return TransportTypeCode.IFRAMEPost
                Else
                    Return TransportTypeCode.XMLHTTP
                End If
            End Get
        End Property

        Public Sub New(ByVal objPage As Page, ByVal eCallBackType As CallBackTypeCode)
            m_objPage = objPage
            CallBackType = eCallBackType
        End Sub

        Public Sub Write()
            Select Case Me.TransportType
                Case TransportTypeCode.IFRAMEPost
                    Dim strContextID As String = m_objPage.Request.Form("ctx")                    'if context passed in then we are using IFRAME Implementation
                    If IsNumeric(strContextID) Then
                        m_objPage.Response.Write("<html><head></head><body onload=""window.parent.dnn.xmlhttp.requests['" & strContextID & "'].complete(window.parent.dnn.dom.getById('txt', document).value);""><form>")
                        m_objPage.Response.Write("<input type=""hidden"" id=""" & ClientAPI.SCRIPT_CALLBACKSTATUSID & """ value=""" & CInt(Me.StatusCode).ToString(CultureInfo.InvariantCulture) & """>")
                        m_objPage.Response.Write("<input type=""hidden"" id=""" & ClientAPI.SCRIPT_CALLBACKSTATUSDESCID & """ value=""" & Me.StatusDesc & """>")
                        m_objPage.Response.Write("<textarea id=""txt"">")
                        m_objPage.Response.Write(HttpUtility.HtmlEncode(MSAJAX.Serialize(New With {.d = Response})))
                        m_objPage.Response.Write("</textarea></body></html>")
                    End If
                Case TransportTypeCode.XMLHTTP
                    m_objPage.Response.AppendHeader(ClientAPI.SCRIPT_CALLBACKSTATUSID, CInt(Me.StatusCode).ToString(CultureInfo.InvariantCulture))
                    m_objPage.Response.AppendHeader(ClientAPI.SCRIPT_CALLBACKSTATUSDESCID, Me.StatusDesc)

                    m_objPage.Response.Write(MSAJAX.Serialize(New With {.d = Response}))    '//don't serialize straight html
            End Select

        End Sub
    End Class

End Namespace
