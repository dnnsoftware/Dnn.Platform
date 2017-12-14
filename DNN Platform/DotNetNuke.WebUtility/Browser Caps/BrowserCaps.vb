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

Imports System
Imports System.IO
Imports System.Web.Caching
Imports System.Xml.Serialization
Imports System.Xml.XPath

Namespace DotNetNuke.UI.Utilities

    <Serializable(), XmlRoot("capabilities")> _
    Public Class BrowserCaps

#Region "Private Members"

        Private m_objFunctionality As FunctionalityCollection
        Private m_objFunctionalityDict As Hashtable
        Private Const CLIENTAPI_CACHE_KEY As String = "ClientAPICaps"

#End Region

#Region "Public Properties"

        <XmlElement("functionality")> _
        Public Property Functionality() As FunctionalityCollection
            Get
                Return m_objFunctionality
            End Get
            Set(ByVal Value As FunctionalityCollection)
                m_objFunctionality = Value
            End Set
        End Property

        Public ReadOnly Property FunctionalityDictionary() As Hashtable
            Get
                If m_objFunctionalityDict Is Nothing Then
                    m_objFunctionalityDict = New Hashtable
                End If
                Return m_objFunctionalityDict
            End Get
        End Property
#End Region

#Region "Private Shared Methods"

        Private Shared Function GetBrowser(ByVal objNav As XPathNavigator) As Browser
            Dim objBrowser As New Browser()
            objBrowser.Contains = objNav.GetAttribute("contains", "")
            objBrowser.Name = objNav.GetAttribute("nm", "")
            Dim strMinVersion As String = objNav.GetAttribute("minversion", "")
            'If Not String.IsNullOrEmpty(strMinVersion) Then    '.NET 2.0 specific
            If Len(strMinVersion) > 0 Then
                objBrowser.MinVersion = Double.Parse(strMinVersion)
            End If
            Return objBrowser
        End Function

#End Region

#Region "Shared Methods"

        Public Shared Function GetBrowserCaps() As BrowserCaps

            Dim objCaps As BrowserCaps = CType(DataCache.GetCache(CLIENTAPI_CACHE_KEY), BrowserCaps)
            Dim objFunc As FunctionalityInfo = Nothing

            If (objCaps Is Nothing) Then
                Dim strPath As String = String.Empty

                Try
                    strPath = System.Web.HttpContext.Current.Server.MapPath(ClientAPI.ScriptPath & "/ClientAPICaps.config")
                Catch ex As Exception
                    'ignore error - worried about people with reverse proxies and such...
                End Try

                Dim objDoc As XPathDocument = Nothing
                Dim objReader As FileStream = Nothing
                Dim tr As TextReader = Nothing
                Dim fileExists As Boolean = File.Exists(strPath)
                Try
                    objCaps = New BrowserCaps()
                    objCaps.Functionality = New FunctionalityCollection()
                    If fileExists Then

                        'Create a FileStream for the Config file
                        objReader = New FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                        objDoc = New XPathDocument(objReader)
                    Else
                        tr = New StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ClientAPICaps.config"))
                        objDoc = New XPathDocument(tr)
                    End If

                    If Not objDoc Is Nothing Then
                        For Each objNavFunc As XPathNavigator In objDoc.CreateNavigator.Select("//functionality")
                            objFunc = New FunctionalityInfo()
                            objFunc.Functionality = DirectCast(System.Enum.Parse(GetType(ClientAPI.ClientFunctionality), objNavFunc.GetAttribute("nm", "")), ClientAPI.ClientFunctionality)
                            objFunc.Desc = objNavFunc.GetAttribute("desc", "")

                            objFunc.Supports = New BrowserCollection()
                            For Each objNavSupports As XPathNavigator In objNavFunc.CreateNavigator.Select("supports/browser")
                                objFunc.Supports.Add(GetBrowser(objNavSupports))
                            Next

                            objFunc.Excludes = New BrowserCollection()
                            For Each objNavExcludes As XPathNavigator In objNavFunc.CreateNavigator.Select("excludes/browser")
                                objFunc.Excludes.Add(GetBrowser(objNavExcludes))
                            Next

                            objCaps.Functionality.Add(objFunc)
                        Next
                    End If
                Catch ex As Exception
                    Throw ex
                Finally
                    If Not objReader Is Nothing Then
                        objReader.Close()
                    End If
                    If Not tr Is Nothing Then
                        tr.Close()
                    End If
                End Try

                ' Set back into Cache
                If fileExists Then
                    DataCache.SetCache(CLIENTAPI_CACHE_KEY, objCaps, New CacheDependency(strPath))
                End If
            End If

            Return objCaps

        End Function

#End Region

    End Class

End Namespace
