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

Imports System.Web.UI
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.Utilities
	Public Class Globals

		''' -----------------------------------------------------------------------------
		''' <summary>
		''' Searches control hierarchy from top down to find a control matching the passed in name
		''' </summary>
		''' <param name="objParent">Root control to begin searching</param>
		''' <param name="strControlName">Name of control to look for</param>
		''' <returns></returns>
		''' <remarks>
		''' This differs from FindControlRecursive in that it looks down the control hierarchy, whereas, the 
		''' FindControlRecursive starts at the passed in control and walks the tree up.  Therefore, this function is 
		''' more a expensive task.
		''' </remarks>
		''' <history>
		''' 	[Jon Henning]	9/17/2004	Created
		'''     [Jon Henning]   12/3/2004   Now checking if the control HasControls before calling FindControl.
		'''                                 Using FindControl or accessing the controls collection on controls like
		'''                                 the DataList can cause problems with ViewState
		''' </history>
		''' -----------------------------------------------------------------------------
		Public Shared Function FindControlRecursive(ByVal objParent As Control, ByVal strControlName As String) As Control
			Return FindControlRecursive(objParent, strControlName, "")
        End Function

        Public Shared Function FindControlRecursive(ByVal objParent As Control, ByVal strControlName As String, ByVal strClientID As String) As Control
            Dim objCtl As Control
            Dim objChild As Control
            objCtl = objParent.FindControl(strControlName)
            If objCtl Is Nothing Then
                For Each objChild In objParent.Controls
                    If objChild.HasControls Then objCtl = FindControlRecursive(objChild, strControlName, strClientID)
                    If Not objCtl Is Nothing AndAlso Len(strClientID) > 0 AndAlso objCtl.ClientID <> strClientID Then objCtl = Nothing
                    If Not objCtl Is Nothing Then Exit For
                Next
            End If
            Return objCtl
        End Function

        Public Shared Function GetAttribute(ByVal objControl As Control, ByVal strAttr As String) As String
            Select Case True
                Case TypeOf objControl Is WebControl
                    Return CType(objControl, WebControl).Attributes(strAttr)
                Case TypeOf objControl Is HtmlControl
                    Return CType(objControl, HtmlControl).Attributes(strAttr)
                Case Else
                    'throw error?
                    Return Nothing
            End Select
        End Function

        Public Shared Sub SetAttribute(ByVal objControl As Control, ByVal strAttr As String, ByVal strValue As String)
            Dim strOrigVal As String = GetAttribute(objControl, strAttr)
            If Len(strOrigVal) > 0 Then strValue = strOrigVal & strValue
            Select Case True
                Case TypeOf objControl Is WebControl
                    Dim objCtl As WebControl = CType(objControl, WebControl)
                    If objCtl.Attributes(strAttr) Is Nothing Then
                        objCtl.Attributes.Add(strAttr, strValue)
                    Else
                        objCtl.Attributes(strAttr) = strValue
                    End If
                Case TypeOf objControl Is HtmlControl
                    Dim objCtl As HtmlControl = CType(objControl, HtmlControl)
                    If objCtl.Attributes(strAttr) Is Nothing Then
                        objCtl.Attributes.Add(strAttr, strValue)
                    Else
                        objCtl.Attributes(strAttr) = strValue
                    End If
                Case Else
                    'throw error?
            End Select
        End Sub

        'hack... can we use a method to determine this
        Private Shared m_aryScripts As Hashtable
        Public Shared Function IsEmbeddedScript(ByVal key As String) As Boolean
            If m_aryScripts Is Nothing Then
                m_aryScripts = New Hashtable
                m_aryScripts.Add("dnn.js", "")
                m_aryScripts.Add("dnn.dom.positioning.js", "")
                m_aryScripts.Add("dnn.diagnostics.js", "")
                m_aryScripts.Add("dnn.scripts.js", "")
                m_aryScripts.Add("dnn.util.tablereorder.js", "")
                m_aryScripts.Add("dnn.xml.js", "")
                m_aryScripts.Add("dnn.xml.jsparser.js", "")
                m_aryScripts.Add("dnn.xmlhttp.js", "")
                m_aryScripts.Add("dnn.xmlhttp.jsxmlhttprequest.js", "")
                m_aryScripts.Add("dnn.motion.js", "")
            End If
            Return m_aryScripts.ContainsKey(key)
        End Function

    End Class
End Namespace