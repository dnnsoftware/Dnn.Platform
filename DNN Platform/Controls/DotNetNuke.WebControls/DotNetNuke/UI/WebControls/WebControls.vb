' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Utilities
Imports DotNetNuke.Web.Client.ClientResourceManagement
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Web.UI
Imports DotNetNuke.Abstractions.ClientResources
Imports DotNetNuke.Internal.SourceGenerators
Imports DotNetNuke.Web.Client.ResourceManager

Namespace DotNetNuke.UI.WebControls
    <StandardModule> _
    Public NotInheritable Class WebControls
        ' Methods
        Private Shared Function FixJs(ByVal js As String) As String
            Return If((String.IsNullOrEmpty(js) OrElse js.EndsWith(";", StringComparison.Ordinal)), js, (js & ";"))
        End Function

        Public Shared Function GetNodeJS(ByVal NavControl As Control, ByVal Node As DNNNode, ByVal DefaultJS As String, ByVal Target As String, ByVal NoneDoesPostBack As Boolean) As String
            Dim expression As String = ""
            If (Strings.Len(Node.JSFunction) > 0) Then
                expression = WebControls.FixJs(Node.JSFunction)
            ElseIf (Strings.Len(DefaultJS) > 0) Then
                expression = WebControls.FixJs(DefaultJS)
            End If
            Select Case Node.ClickAction
                Case eClickAction.PostBack, eClickAction.Expand, eClickAction.None
                    If ((Node.ClickAction <> eClickAction.None) OrElse NoneDoesPostBack) Then
                        If (Strings.Len(expression) > 0) Then
                            expression = ("if (eval(""" & expression.Replace("""", """""") & """) != false) ")
                        End If
                        expression = (expression & NavControl.Page.ClientScript.GetPostBackEventReference(NavControl, (Node.ID & ClientAPI.COLUMN_DELIMITER & "Click")))
                    End If
                    Exit Select
                Case eClickAction.Navigate
                    If (Strings.Len(expression) > 0) Then
                        expression = ("if (eval(""" & expression.Replace("""", """""") & """) != false) ")
                    End If
                    If (Strings.Len(Target) > 0) Then
                        Dim strArray As String() = New String() { expression, "window.frames.", Target, ".location.href='", Node.NavigateURL, "'; void(0);" }
                        expression = String.Concat(strArray)
                    ElseIf Not String.IsNullOrEmpty(expression) Then
                        expression = (expression & "window.location.href='" & Node.NavigateURL & "';")
                    End If
                    Exit Select
                Case Else
                    Exit Select
            End Select
            Return expression
        End Function

        Public Shared Function IsDesignMode() As Boolean
            Return Object.ReferenceEquals(HttpContext.Current, Nothing)
        End Function

        
        <Obsolete("Deprecated in DotNetNuke 10.2.3. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")>
        Public Shared Sub RegisterClientScriptBlock(ByVal ThePage As Page, ByVal Key As String)
            If ClientAPI.UseExternalScripts Then
#Disable Warning BC40000
                ClientResourceManager.RegisterScript(ThePage, (ClientAPI.ScriptPath & Key))
#Enable Warning BC40000
            Else
                MSAJAX.RegisterClientScript(ThePage, Key, "DotNetNuke.WebControls")
            End If
        End Sub

        Public Shared Sub RegisterClientScriptBlock(clientResourceController As IClientResourceController, thePage As Page, key As String)
            If ClientAPI.UseExternalScripts Then
                clientResourceController.CreateScript(ClientAPI.ScriptPath & key).Register()
            Else
                MSAJAX.RegisterClientScript(thePage, key, "DotNetNuke.WebControls")
            End If
        End Sub

        <CLSCompliant(false)>
        <Obsolete("Deprecated in DotNetNuke 10.2.3. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")>
        Public Shared Sub RegisterClientScriptBlock(ByVal ThePage As Page, ByVal Key As String, ByVal priority As DotNetNuke.Web.Client.FileOrder.Js)
            If ClientAPI.UseExternalScripts Then
#Disable Warning BC40000
                ClientResourceManager.RegisterScript(ThePage, (ClientAPI.ScriptPath & Key), priority)
#Enable Warning BC40000
            Else
                MSAJAX.RegisterClientScript(ThePage, Key, "DotNetNuke.WebControls")
            End If
        End Sub

        <CLSCompliant(False)>
        Public Shared Sub RegisterClientScriptBlock(clientResourceController As IClientResourceController, thePage As Page, key As String, priority As DotNetNuke.Web.Client.FileOrder.Js)
            If ClientAPI.UseExternalScripts Then
                clientResourceController.CreateScript(ClientAPI.ScriptPath & Key).SetPriority(priority).Register()
            Else
                MSAJAX.RegisterClientScript(ThePage, Key, "DotNetNuke.WebControls")
            End If
        End Sub

        <Obsolete("Deprecated in DotNetNuke 10.2.3. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")>
        Public Shared Sub RegisterSubmitComponent(ByVal ThePage As Page)
            If Not ThePage.ClientScript.IsOnSubmitStatementRegistered(ThePage.GetType, "dnn.controls.submitComp") Then
                WebControls.RegisterClientScriptBlock(ThePage, "dnn.controls.js", DotNetNuke.Web.Client.FileOrder.Js.DnnControlsLabelEdit)
                ThePage.ClientScript.RegisterOnSubmitStatement(ThePage.GetType, "dnn.controls.submitComp", "dnn.controls.submitComp.onsubmit()")
            End If
        End Sub

        Public Shared Sub RegisterSubmitComponent(clientResourceController As IClientResourceController, thePage As Page)
            If Not ThePage.ClientScript.IsOnSubmitStatementRegistered(thePage.GetType, "dnn.controls.submitComp") Then
                WebControls.RegisterClientScriptBlock(clientResourceController, thePage, "dnn.controls.js", DotNetNuke.Web.Client.FileOrder.Js.DnnControlsLabelEdit)
                ThePage.ClientScript.RegisterOnSubmitStatement(thePage.GetType, "dnn.controls.submitComp", "dnn.controls.submitComp.onsubmit()")
            End If
        End Sub

        Public Shared Function StripUrlPaths(ByVal [Text] As String, ByVal Type As UrlFormatType, ByVal ThePage As Page) As String
            Dim str2 As String
            If Object.ReferenceEquals(ThePage, Nothing) Then
                str2 = [Text]
            Else
                Dim str3 As String = Nothing
                Dim absoluteUri As String
                Select Case Type
                    Case UrlFormatType.AbsoluteWithoutServer
                        absoluteUri = ThePage.Request.Url.AbsoluteUri
                        str3 = absoluteUri.Substring(0, (absoluteUri.Length - ThePage.Request.Url.PathAndQuery.Length))
                        Exit Select
                    Case UrlFormatType.Relative
                        absoluteUri = ThePage.Request.Path.Substring((ThePage.Request.Path.LastIndexOf("/", StringComparison.Ordinal) + 1))
                        str3 = ThePage.Request.Url.AbsoluteUri.Substring(0, ThePage.Request.Url.AbsoluteUri.IndexOf(absoluteUri, StringComparison.Ordinal))
                        Exit Select
                    Case Else
                        Exit Select
                End Select
                str3 = Regex.Escape(str3)
                Dim pattern As String = $"(?i:(?<=(?:src|href)=(?:""|')){str3}(?=[^'|""]*?(?:""|')))"
                str2 = Regex.Replace([Text], pattern, "")
            End If
            Return str2
        End Function

        Public Shared Function ToJSON(ByVal hash As Hashtable) As String
            Dim enumerator As IEnumerator = Nothing
            Dim builder As New StringBuilder
            Dim s As String = ""
            builder.Append("{"c)
            Try 
                enumerator = hash.Keys.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim str3 As String = Conversions.ToString(enumerator.Current)
                    If (builder.Length > 1) Then
                        builder.Append(","c)
                    End If
                    If Not True Then
                        s = Conversions.ToString(hash(str3))
                    Else
                        s = Conversions.ToString(hash(str3))
                        s = ("'" & ClientAPI.EscapeForJavascript(s) & "'")
                    End If
                    builder.Append((str3 & ":" & s))
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            builder.Append("}"c)
            Return builder.ToString
        End Function

    End Class
End Namespace

