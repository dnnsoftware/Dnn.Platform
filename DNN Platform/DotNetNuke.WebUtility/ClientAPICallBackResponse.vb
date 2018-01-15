'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2018
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
            XMLHTTP
            IFRAMEPost
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
                        m_objPage.Response.Write("<input type=""hidden"" id=""" & ClientAPI.SCRIPT_CALLBACKSTATUSID & """ value=""" & CInt(Me.StatusCode).ToString & """>")
                        m_objPage.Response.Write("<input type=""hidden"" id=""" & ClientAPI.SCRIPT_CALLBACKSTATUSDESCID & """ value=""" & Me.StatusDesc & """>")
                        m_objPage.Response.Write("<textarea id=""txt"">")
                        m_objPage.Response.Write(HttpUtility.HtmlEncode(MSAJAX.Serialize(New With {.d = Response})))
                        m_objPage.Response.Write("</textarea></body></html>")
                    End If
                Case TransportTypeCode.XMLHTTP
                    m_objPage.Response.AppendHeader(ClientAPI.SCRIPT_CALLBACKSTATUSID, CInt(Me.StatusCode).ToString)
                    m_objPage.Response.AppendHeader(ClientAPI.SCRIPT_CALLBACKSTATUSDESCID, Me.StatusDesc)

                    m_objPage.Response.Write(MSAJAX.Serialize(New With {.d = Response}))    '//don't serialize straight html
            End Select

        End Sub
    End Class

End Namespace
