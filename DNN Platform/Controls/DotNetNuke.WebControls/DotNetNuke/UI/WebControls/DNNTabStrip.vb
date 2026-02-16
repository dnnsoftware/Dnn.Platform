' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Design.WebControls
Imports DotNetNuke.UI.Utilities
Imports DotNetNuke.Web.Client
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    <DefaultProperty("Tabs"), ParseChildren(True, "Tabs"), Designer(GetType(DNNTabStripDesigner)), ToolboxData("<{0}:DNNTabStrip runat=server><{0}:DNNTab id=""Tab1"" Text=""Tab 1"" runat=""server"">Tab 1 Content</{0}:DNNTab><{0}:DNNTab id=""Tab2"" Text=""Tab 2"" runat=""server"">Tab 2 Content</{0}:DNNTab></{0}:DNNTabStrip>")> _
    Public Class DNNTabStrip
        Inherits WebControl
        Implements IClientAPICallbackEventHandler, IPostBackEventHandler, IPostBackDataHandler
        ' Methods
        Public Sub New()
            MyBase.New(HtmlTextWriterTag.Div)
            AddHandler MyBase.Init, New EventHandler(AddressOf Me.DNNTabStrip_Init)
            AddHandler MyBase.PreRender, New EventHandler(AddressOf Me.DNNTabStrip_PreRender)
            DNNTabStrip.__ENCAddToList(Me)
            Me.m_objPostedTabData = New Hashtable
            Me.m_objDefaultDNNTabLabel = New DNNTabLabel
            Me.m_iSelectedIndex = -1
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
            SyncLock DNNTabStrip.__ENCList
                If (DNNTabStrip.__ENCList.Count = DNNTabStrip.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNTabStrip.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNTabStrip.__ENCList.RemoveRange(index, (DNNTabStrip.__ENCList.Count - index))
                            DNNTabStrip.__ENCList.Capacity = DNNTabStrip.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNTabStrip.__ENCList(num2)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNTabStrip.__ENCList(index) = DNNTabStrip.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNTabStrip.__ENCList.Add(New WeakReference(value))
            End SyncLock
        End Sub

        Protected Overrides Sub AddAttributesToRender(ByVal writer As HtmlTextWriter)
            MyBase.AddAttributesToRender(writer)
        End Sub

        Protected Overrides Sub AddParsedSubObject(ByVal obj As Object)
            If TypeOf obj Is DNNTab Then
                MyBase.AddParsedSubObject(obj)
            End If
        End Sub

        Private Sub AssignSelectedIndex()
            Dim num2 As Integer = (Me.Tabs.Count - 1)
            Dim num As Integer = 0
            Do While True
                Dim num3 As Integer = num2
                If (num > num3) Then
                    Return
                End If
                If Me.m_objPostedTabData.ContainsKey(Me.Tabs(num).ID) Then
                    If ((Conversions.ToInteger(Me.m_objPostedTabData(Me.Tabs(num).ID)) And 2) <> 0) Then
                        Me.SelectedIndex = num
                    End If
                    If ((Conversions.ToInteger(Me.m_objPostedTabData(Me.Tabs(num).ID)) And 1) <> 0) Then
                        Me.Tabs(num).SetTabState(True)
                    End If
                    Me.Tabs(num).Enabled = ((Conversions.ToInteger(Me.m_objPostedTabData(Me.Tabs(num).ID)) And 4) <> 0)
                End If
                num += 1
            Loop
        End Sub

        Private Sub DNNTabStrip_Init(ByVal sender As Object, ByVal e As EventArgs)
            If ClientAPI.NeedsDNNVariable(Me) Then
                AddHandler Me.Page.Load, New EventHandler(AddressOf Me.ParentOnLoad)
            End If
            Me.LoadPostedVars
            Me.RaisePreLoadPostDataEvents
        End Sub

        Private Sub DNNTabStrip_PreRender(ByVal sender As Object, ByVal e As EventArgs)
            Me.RegisterClientScript
            Me.Page.RegisterRequiresPostBack(Me)
            If ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML) Then
                ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn)
                ClientAPI.RegisterClientVariable(Me.Page, (Me.ClientID & "_json"), ("{" & Me.Tabs.ToJSON & "}"), True)
            End If
        End Sub

        Private Shared Function FindForm(ByVal tabStrip As DNNTabStrip) As HtmlForm
            Dim oCtl As Control = tabStrip
            Do While True
                Dim form As HtmlForm
                If TypeOf oCtl Is HtmlForm Then
                    form = DirectCast(oCtl, HtmlForm)
                Else
                    If (If(((oCtl Is Nothing) OrElse TypeOf oCtl Is Page), 1, 0) = 0) Then
                        oCtl = oCtl.Parent
                        Continue Do
                    End If
                    form = Nothing
                End If
                Return form
            Loop

            Return Nothing
        End Function

        Private Function GetFormHTML() As String
            Dim sb As New StringBuilder
            Dim writer As New HtmlTextWriter(New StringWriter(sb))
            Dim str2 As String = ""
            Dim objA As Control = DNNTabStrip.FindForm(Me)
            If Not Object.ReferenceEquals(objA, Nothing) Then
                objA.Controls.Clear
                objA.RenderControl(writer)
                str2 = sb.ToString
            End If
            Return str2
        End Function

        Private Function GetScripts(ByVal HTML As String) As HTMLElementCollection
            Dim enumerator As IEnumerator = Nothing
            Dim regex As New Regex("(?i:" & ChrW(9) & "(?<element>(?:<script" & ChrW(9) & ChrW(9) & "(?:\s*" & ChrW(9) & ChrW(9) & "(?:" & ChrW(9) & ChrW(9) & ChrW(9) & "(?<attr>[^=>]*?)" & ChrW(9) & ChrW(9) & ChrW(9) & "=(?:""|')" & ChrW(9) & ChrW(9) & ChrW(9) & "(?<attrv>[^""|']*?)" & ChrW(9) & ChrW(9) & ChrW(9) & "(?:""|')" & ChrW(9) & ChrW(9) & "))*        )" & ChrW(9) & "((?(?=\s*?/>)\s*?/>|                (?:\s*?>" & ChrW(9) & "(?:[\s\r\n]*?<!--)?(?<text>[\s\S]*?)                </script>))" & ChrW(9) & ")))", (RegexOptions.IgnorePatternWhitespace Or (RegexOptions.Singleline Or RegexOptions.IgnoreCase)))
            Dim elements2 As New HTMLElementCollection
            Try 
                enumerator = regex.Matches(HTML).GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As Match = DirectCast(enumerator.Current, Match)
                    Dim objA As HTMLElement = Nothing
                    Dim num3 As Integer = (current.Groups.Count - 1)
                    Dim i As Integer = 0
                    Do While True
                        Dim collection As Collection = Nothing
                        Dim enumerator2 As IEnumerator = Nothing
                        Dim num4 As Integer = num3
                        If (i > num4) Then
                            If Not Object.ReferenceEquals(objA, Nothing) Then
                                elements2.Add(objA)
                            End If
                            Exit Do
                        End If
                        Dim str2 As String = regex.GroupNameFromNumber(i)
                        If (str2 = "attr") Then
                            collection = New Collection
                        End If
                        Dim num As Integer = 1
                        Try 
                            enumerator2 = current.Groups(i).Captures.GetEnumerator
                            Do While True
                                If Not enumerator2.MoveNext Then
                                    Exit Do
                                End If
                                Dim capture As Capture = DirectCast(enumerator2.Current, Capture)
                                Dim str3 As String = str2
                                If (str3 = "element") Then
                                    Dim htmlElement As New HTMLElement(capture.Value)
                                    htmlElement.Raw = current.Value
                                    Continue Do
                                End If
                                If (str3 = "attr") Then
                                    collection.Add(capture.Value, Nothing, Nothing, Nothing)
                                    Continue Do
                                End If
                                If (str3 <> "attrv") Then
                                    If (str3 <> "text") Then
                                        Continue Do
                                    End If
                                    objA.Text = capture.Value
                                    Continue Do
                                End If
                                If (Conversions.ToString(collection(num)) = "src") Then
                                    objA.Attributes.Add(collection(num), HttpUtility.HtmlDecode(capture.Value))
                                Else
                                    objA.Attributes.Add(collection(num), capture.Value)
                                End If
                                num += 1
                            Loop
                        Finally
                            If Not Object.ReferenceEquals(TryCast(enumerator2,IDisposable), Nothing) Then
                                TryCast(enumerator2,IDisposable).Dispose
                            End If
                        End Try
                        i += 1
                    Loop
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            Return elements2
        End Function

        Private Function GetTabContents(ByVal objTab As DNNTab) As String
            objTab.RaiseSetupDefaultsEvent(True)
            Dim sb As New StringBuilder
            Dim writer As New HtmlTextWriter(New StringWriter(sb))
            Dim objPage As Page = Me.Page
            Dim objA As HtmlForm = DNNTabStrip.FindForm(Me)
            If Not Object.ReferenceEquals(objA, Nothing) Then
                objA.Controls.Clear
            Else
                objA = New HtmlForm
                objPage.Controls.Add(objA)
            End If
            objTab.ID = objTab.UniqueID
            objA.Controls.Add(New LiteralControl("~`~`~`~`~`~`"))
            objA.Controls.Add(objTab)
            objA.Controls.Add(New LiteralControl("~`~`~`~`~`~`"))
            objTab.SetParent(Me)
            objTab.Enabled = True
            objA.RenderControl(writer)
            Dim dictionary As New Dictionary(Of String, String)
            Dim str2 As String = Strings.Split(sb.ToString, "~`~`~`~`~`~`", -1, CompareMethod.Binary)(1)
            dictionary.Add("text", str2)
            Dim scripts As HTMLElementCollection = Me.GetScripts(sb.ToString)
            If (scripts.Count > 0) Then
                dictionary.Add("scripts", scripts.ToJSON)
            End If
            Dim clientVariableList As Dictionary(Of String, String) = ClientAPI.GetClientVariableList(objPage)
            dictionary.Add("vars", MSAJAX.Serialize(clientVariableList))
            Return MSAJAX.Serialize(dictionary)
        End Function

        Private Function GetTabIds() As String
            Dim enumerator As IEnumerator = Nothing
            Dim expression As String = ""
            Try 
                enumerator = Me.Tabs.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As DNNTab = DirectCast(enumerator.Current, DNNTab)
                    expression = (expression & current.ClientID & ",")
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            Return If((Strings.Len(expression) <= 0), "", expression.Substring(0, (expression.Length - 1)))
        End Function

        Public Function LoadPostData(ByVal postDataKey As String, ByVal postCollection As NameValueCollection) As Boolean Implements IPostBackDataHandler.LoadPostData
            Dim flag As Boolean
            Return flag
        End Function

        Private Sub LoadPostedVars()
            Dim strArray3 As String() = Strings.Split(ClientAPI.GetClientVariable(Me.Page, (Me.ClientID & "_tabs")), ",", -1, CompareMethod.Binary)
            Dim index As Integer = 0
            Do While True
                If (index >= strArray3.Length) Then
                    Me.AssignSelectedIndex
                    Return
                End If
                Dim expression As String = strArray3(index)
                Dim strArray2 As String() = Strings.Split(expression, "=", -1, CompareMethod.Binary)
                If (strArray2.Length = 2) Then
                    Me.m_objPostedTabData.Add(strArray2(0), strArray2(1))
                End If
                index += 1
            Loop
        End Sub

        Protected Function MarshalledProperties() As Hashtable
            Dim hashtable2 As New Hashtable From { { "tabs", Me.GetTabIds } }
            If Not Me.IsDownLevel Then
                Select Case Me.TabRenderMode
                    Case eTabRenderMode.PostBack
                        hashtable2.Add("callback", ClientAPI.GetPostBackClientHyperlink(Me, ("[TABID]" & ClientAPI.COLUMN_DELIMITER & "OnDemand")))
                        Exit Select
                    Case eTabRenderMode.CallBack
                        If ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.XMLHTTP) Then
                            Dim str As String = ClientAPI.GetCallbackEventReference(Me, "'[TABID]'", "this.callBackSuccess", "this", "this.callBackFail", "this.callBackStatus", "[POST]", ClientAPICallBackResponse.CallBackTypeCode.Simple)
                            Dim length As Integer = ((str.IndexOf("'[POST]'", StringComparison.Ordinal) + "'[POST]'".Length) + 1)
                            str = (str.Substring(0, length) & "'[CBTYPE]'" & str.Substring((length + 1)))
                            hashtable2.Add("callback", str)
                        ElseIf Me.IsDownLevel Then
                            hashtable2.Add("callback", ClientAPI.GetPostBackClientHyperlink(Me, ("[TABID]" & ClientAPI.COLUMN_DELIMITER & "OnDemand")))
                        End If
                        Exit Select
                    Case Else
                        Exit Select
                End Select
            End If
            Return hashtable2
        End Function

        Private Sub ParentOnLoad(ByVal Sender As Object, ByVal e As EventArgs)
            ClientAPI.RegisterDNNVariableControl(Me)
        End Sub

        Public Function RaiseClientAPICallbackEvent(ByVal eventArgument As String) As String Implements IClientAPICallbackEventHandler.RaiseClientAPICallbackEvent
            Dim tabContents As String = Nothing
            Dim objA As DNNTab = Me.Tabs.FindTab(eventArgument)
            If Not Object.ReferenceEquals(objA, Nothing) Then
                tabContents = Me.GetTabContents(objA)
            End If
            Return tabContents
        End Function

        Public Overridable Sub RaisePostBackEvent(ByVal eventArgument As String) Implements IPostBackEventHandler.RaisePostBackEvent
            Dim strArray As String() = Strings.Split(eventArgument, ClientAPI.COLUMN_DELIMITER, -1, CompareMethod.Binary)
            Dim objA As DNNTab = Me.Tabs.FindTab(strArray(0))
            If Not Object.ReferenceEquals(objA, Nothing) Then
                If (strArray.Length <= 1) Then
                    Me.SelectedTab = objA
                ElseIf (strArray(1) = "OnDemand") Then
                    Me.SelectedTab = objA
                End If
            End If
        End Sub

        Public Sub RaisePostDataChangedEvent() Implements IPostBackDataHandler.RaisePostDataChangedEvent
        End Sub

        Private Sub RaisePreLoadPostDataEvents()
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.Tabs.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As DNNTab = DirectCast(enumerator.Current, DNNTab)
                    If current.IsPostBack Then
                        current.RaisePreLoadPostData
                    End If
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
        End Sub

        Private Sub RegisterClientScript()
            Dim flag2 As Boolean = Not Me.IsDownLevel
            If flag2 Then
                Dim enumerator As IEnumerator = Nothing
                ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_dom)
                Dim flag As Boolean = (Me.TabRenderMode = eTabRenderMode.CallBack)
                Try 
                    enumerator = Me.Tabs.GetEnumerator
                    Do While True
                        flag2 = enumerator.MoveNext
                        If flag2 Then
                            Dim current As DNNTab = DirectCast(enumerator.Current, DNNTab)
                            If (current.TabRenderMode = eTabRenderMode.CallBack) Then
                                flag = True
                            End If
                            If Not flag Then
                                Continue Do
                            End If
                        End If
                        Exit Do
                    Loop
                Finally
                    If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                        TryCast(enumerator,IDisposable).Dispose
                    End If
                End Try
                If (If((Not flag OrElse Not ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.XMLHTTP)), 0, 1) <> 0) Then
                    ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_xmlhttp)
                End If
                WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.js", FileOrder.Js.DnnControlsLabelEdit)
                WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.dnntabstrip.js")
            End If
        End Sub

        Protected Overrides Sub Render(ByVal writer As HtmlTextWriter)
            MyBase.Render(writer)
            If Not Me.IsDownLevel Then
                Dim properties As Hashtable = Me.MarshalledProperties
                ScriptGenerator.GetMarshalledPropertyJSON(Me.m_objDefaultDNNTabLabel, properties)
                Dim marshalledPropertyJSON As String = ScriptGenerator.GetMarshalledPropertyJSON(Me, properties)
                BaseWebControl.RegisterInitialize(Me, "initTabStrip", marshalledPropertyJSON)
            End If
        End Sub

        Protected Overrides Sub RenderContents(ByVal writer As HtmlTextWriter)
            Dim enumerator As IEnumerator = Nothing
            Select Case Me.TabAlignment
                Case Alignment.Top, Alignment.Left
                    Me.RenderTabs(writer)
                    Exit Select
                Case Else
                    Exit Select
            End Select
            writer.AddAttribute("id", (Me.ClientID & "_c"))
            If (Strings.Len(Me.CssContentContainer) > 0) Then
                writer.AddAttribute("class", Me.CssContentContainer)
            End If
            writer.RenderBeginTag("div")
            Try 
                enumerator = Me.Tabs.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As DNNTab = DirectCast(enumerator.Current, DNNTab)
                    If (current.IsPostBack OrElse (current.IsSelected OrElse (current.TabRenderMode = eTabRenderMode.All))) Then
                        current.RaiseSetupDefaultsEvent(False)
                        If Not current.Visible Then
                            current.Visible = True
                            current.Style.Add("display", "none")
                        ElseIf (current.IsSelected AndAlso Not Object.ReferenceEquals(current.Style("display"), Nothing)) Then
                            current.Style.Remove("display")
                        End If
                        current.RenderControl(writer)
                    End If
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            writer.RenderEndTag
            Select Case Me.TabAlignment
                Case Alignment.Bottom, Alignment.Right
                    Me.RenderTabs(writer)
                    Exit Select
                Case Else
                    Exit Select
            End Select
        End Sub

        Private Sub RenderTabs(ByVal writer As HtmlTextWriter)
            Dim enumerator As IEnumerator = Nothing
            writer.AddAttribute("id", (Me.ClientID & "_lc"))
            If (Strings.Len(Me.CssTabContainer) > 0) Then
                writer.AddAttribute("class", Me.CssTabContainer)
            End If
            writer.RenderBeginTag("div ")
            Try 
                enumerator = Me.Tabs.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    DirectCast(enumerator.Current, DNNTab).RenderLabel(writer)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            writer.RenderEndTag
        End Sub


        ' Properties
        <Bindable(True), ClientProperty, DefaultValue(""), ClientPropertyName("bid")> _
        Public Property BehaviorID As String
            Get
                Return Conversions.ToString(Me.ViewState("BehaviorID"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("BehaviorID") = Value
            End Set
        End Property

        <Bindable(True), PersistenceMode(PersistenceMode.Attribute), ClientPropertyName("callbackSF"), ClientProperty, Category("Behavior")> _
        Public Property CallbackStatusFunction As String
            Get
                Return Conversions.ToString(Me.ViewState("CallbackStatusFunction"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("CallbackStatusFunction") = Value
            End Set
        End Property

        <DefaultValue(""), Bindable(True), ClientPropertyName("tabClickF"), ClientProperty, Category("Behavior"), PersistenceMode(PersistenceMode.Attribute)> _
        Public Property TabClickFunction As String
            Get
                Return Conversions.ToString(Me.ViewState("TabClickFunction"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("TabClickFunction") = Value
            End Set
        End Property

        <Bindable(True), Category("Behavior"), PersistenceMode(PersistenceMode.Attribute), ClientProperty, DefaultValue(""), ClientPropertyName("selIdxF")> _
        Public Property SelectedIndexChangedFunction As String
            Get
                Return Conversions.ToString(Me.ViewState("SelectedIndexChangedFunction"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("SelectedIndexChangedFunction") = Value
            End Set
        End Property

        Public Property ClientAPIScriptPath As String
            Get
                Return ClientAPI.ScriptPath
            End Get
            Set(ByVal Value As String)
                ClientAPI.ScriptPath = Value
            End Set
        End Property

        Public Property TabStripScriptPath As String
            Get
                Return If((Strings.Len(Me.ViewState("TabScriptPath")) <> 0), Conversions.ToString(Me.ViewState("TabScriptPath")), Me.ClientAPIScriptPath)
            End Get
            Set(ByVal Value As String)
                Me.ViewState("TabScriptPath") = Value
            End Set
        End Property

        <DefaultValue(False), Category("Behavior")> _
        Public Property ForceDownLevel As Boolean
            Get
                Return Conversions.ToBoolean(Me.ViewState("ForceDownLevel"))
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("ForceDownLevel") = Value
            End Set
        End Property

        <Category("Behavior"), DefaultValue(False)> _
        Public Property IsCrawler As Boolean
            Get
                Return If(((Strings.Len(Me.ViewState("IsCrawler")) <> 0) OrElse (HttpContext.Current Is Nothing)), Conversions.ToBoolean(Me.ViewState("IsCrawler")), HttpContext.Current.Request.Browser.Crawler)
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("IsCrawler") = Value
            End Set
        End Property

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
        Public ReadOnly Property IsDownLevel As Boolean
            Get
                Return (Me.ForceDownLevel OrElse (Me.IsCrawler OrElse Not ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML)))
            End Get
        End Property

        <Category("Behavior"), ClientProperty, DefaultValue(2), ClientPropertyName("trm")> _
        Public Property TabRenderMode As eTabRenderMode
            Get
                Return If((Not Me.ViewState("TabRenderMode") Is Nothing), DirectCast(Conversions.ToInteger(Me.ViewState("TabRenderMode")), eTabRenderMode), eTabRenderMode.CallBack)
            End Get
            Set(ByVal Value As eTabRenderMode)
                Me.ViewState("TabRenderMode") = Value
            End Set
        End Property

        <RefreshProperties(RefreshProperties.All), DefaultValue(0)> _
        Public Property SelectedIndex As Integer
            Get
                Return If((Me.m_iSelectedIndex = -1), If((Strings.Len(Me.ViewState("SelectedIndex")) <> 0), Conversions.ToInteger(Me.ViewState("SelectedIndex")), 0), Me.m_iSelectedIndex)
            End Get
            Set(ByVal Value As Integer)
                If (If(((Me.m_objTabs Is Nothing) OrElse (Value <= (Me.Tabs.Count - 1))), 0, 1) <> 0) Then
                    Throw New InvalidOperationException("Invalid SelectedIndex")
                End If
                Me.m_iSelectedIndex = Value
                Me.ViewState("SelectedIndex") = Value
            End Set
        End Property

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
        Public Property SelectedTab As DNNTab
            Get
                Return If((Me.Tabs.Count <= Me.SelectedIndex), Nothing, Me.Tabs(Me.SelectedIndex))
            End Get
            Set(ByVal value As DNNTab)
                Dim num2 As Integer = (Me.Tabs.Count - 1)
                Dim num As Integer = 0
                Do While True
                    Dim num3 As Integer = num2
                    If (num <= num3) Then
                        If (Me.Tabs(num).ID <> value.ID) Then
                            num += 1
                            Continue Do
                        End If
                        Me.SelectedIndex = num
                    End If
                    Return
                Loop
            End Set
        End Property

        <PersistenceMode(PersistenceMode.InnerDefaultProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), MergableProperty(False)> _
        Public Overridable ReadOnly Property Tabs As TabStripTabCollection
            Get
                If Object.ReferenceEquals(Me.m_objTabs, Nothing) Then
                    Me.m_objTabs = New TabStripTabCollection(Me)
                End If
                Return Me.m_objTabs
            End Get
        End Property

        <Description("The style to be applied to Label"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Appearance"), NotifyParentProperty(True)> _
        Public ReadOnly Property DefaultLabel As DNNTabLabel
            Get
                Return Me.m_objDefaultDNNTabLabel
            End Get
        End Property

        <Category("Appearance")> _
        Public Property DefaultContainerCssClass As String
            Get
                Return If((Not Me.ViewState("DefaultContainerCssClass") Is Nothing), Conversions.ToString(Me.ViewState("DefaultContainerCssClass")), "")
            End Get
            Set(ByVal Value As String)
                Me.ViewState("DefaultContainerCssClass") = Value
            End Set
        End Property

        <Category("Appearance")> _
        Public Property CssTabContainer As String
            Get
                Return If((Not Me.ViewState("CssTabContainer") Is Nothing), Conversions.ToString(Me.ViewState("CssTabContainer")), "")
            End Get
            Set(ByVal Value As String)
                Me.ViewState("CssTabContainer") = Value
            End Set
        End Property

        <Category("Appearance")> _
        Public Property CssContentContainer As String
            Get
                Return If((Not Me.ViewState("CssContentContainer") Is Nothing), Conversions.ToString(Me.ViewState("CssContentContainer")), "")
            End Get
            Set(ByVal Value As String)
                Me.ViewState("CssContentContainer") = Value
            End Set
        End Property

        <DefaultValue(""), Description("Image to display during a callback"), Bindable(True), ClientPropertyName("workImage"), ClientProperty> _
        Public Property WorkImage As String
            Get
                Return Conversions.ToString(Me.ViewState("WorkImage"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("WorkImage") = Value
            End Set
        End Property

        <ClientPropertyName("cbtype"), Category("Behavior"), DefaultValue(0), ClientProperty, Bindable(True), Description("Image to display during a callback")> _
        Public Property CallBackType As ClientAPICallBackResponse.CallBackTypeCode
            Get
                Return If((Strings.Len(Me.ViewState("CallBackTypeCode")) <= 0), ClientAPICallBackResponse.CallBackTypeCode.Simple, DirectCast(Conversions.ToInteger(Me.ViewState("CallBackTypeCode")), ClientAPICallBackResponse.CallBackTypeCode))
            End Get
            Set(ByVal Value As ClientAPICallBackResponse.CallBackTypeCode)
                Me.ViewState("CallBackTypeCode") = Value
            End Set
        End Property

        <PersistenceMode(PersistenceMode.Attribute), DefaultValue(0), Bindable(True)> _
        Public Property TabAlignment As Alignment
            Get
                Return If((Strings.Len(Me.ViewState("TabAlignment")) <= 0), Alignment.Top, DirectCast(Conversions.ToInteger(Me.ViewState("TabAlignment")), Alignment))
            End Get
            Set(ByVal Value As Alignment)
                Me.ViewState("TabAlignment") = Value
            End Set
        End Property


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private Const TAB_CONTENT_DELIMITER As String = "~`~`~`~`~`~`"
        Private m_objTabs As TabStripTabCollection
        Private m_objPostedTabData As Hashtable
        Private m_objDefaultDNNTabLabel As DNNTabLabel
        Private Const TAB_RENDERED As Integer = 1
        Private Const TAB_SELECTED As Integer = 2
        Private Const TAB_ENABLED As Integer = 4
        Private m_iSelectedIndex As Integer

        ' Nested Types
        Public Enum eTabRenderMode
            ' Fields
            All = 0
            PostBack = 1
            CallBack = 2
        End Enum
    End Class
End Namespace

