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
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Runtime.CompilerServices
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    <Designer(GetType(DNNTextSuggestDesigner)), ToolboxData("<{0}:DNNTextSuggest runat=server></{0}:DNNTextSuggest>")> _
    Public Class DNNTextSuggest
        Inherits TextBox
        Implements IPostBackEventHandler, IClientAPICallbackEventHandler
        ' Events
        Public Event NodeClick As DNNDNNNodeClickHandler
        Public Event PopulateOnDemand As DNNTextSuggestEventHandler

        ' Methods
        Public Sub New()
            AddHandler MyBase.PreRender, New EventHandler(AddressOf Me.DNNTextSuggest_PreRender)
            DNNTextSuggest.__ENCAddToList(Me)
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
            SyncLock DNNTextSuggest.__ENCList
                If (DNNTextSuggest.__ENCList.Count = DNNTextSuggest.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNTextSuggest.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNTextSuggest.__ENCList.RemoveRange(index, (DNNTextSuggest.__ENCList.Count - index))
                            DNNTextSuggest.__ENCList.Capacity = DNNTextSuggest.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNTextSuggest.__ENCList(num2)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNTextSuggest.__ENCList(index) = DNNTextSuggest.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNTextSuggest.__ENCList.Add(New WeakReference(value))
            End SyncLock
        End Sub

        Private Sub DNNTextSuggest_PreRender(ByVal sender As Object, ByVal e As EventArgs)
            Me.RegisterClientScript
            Me.Page.RegisterRequiresPostBack(Me)
        End Sub

        Public Function FindNode(ByVal strID As String) As DNNNode
            Return Me.DNNNodes.FindNode(strID)
        End Function

        Public Function FindNodeByKey(ByVal strKey As String) As DNNNode
            Return Me.DNNNodes.FindNodeByKey(strKey)
        End Function

        Public Sub LoadXml(ByVal strXml As String)
            Me.m_objNodes = New DNNNodeCollection(strXml, "")
        End Sub

        Protected Function MarshalledProperties() As Hashtable
            Dim hashtable2 As New Hashtable
            Select Case Me.IDToken
                Case eIDTokenChar.Brackets
                    hashtable2.Add("idtok", "[~]")
                    Exit Select
                Case eIDTokenChar.Paranthesis
                    hashtable2.Add("idtok", "(~)")
                    Exit Select
                Case Else
                    Exit Select
            End Select
            hashtable2.Add("postback", ClientAPI.GetPostBackEventReference(Me, ("[TEXT]" & ClientAPI.COLUMN_DELIMITER & "Click")))
            If ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.XMLHTTP) Then
                hashtable2.Add("callback", ClientAPI.GetCallbackEventReference(Me, "this.getText()", "this.callBackSuccess", "this", "this.callBackFail", "this.callBackStatus", Me.CallBackPostChildrenOf, Me.CallBackType))
            End If
            Return hashtable2
        End Function

        Public Overridable Sub OnNodeClick(ByVal e As DNNTextSuggestEventArgs)
            Dim nodeClickEvent As DNNDNNNodeClickHandler = Me.NodeClickEvent
            If Not Object.ReferenceEquals(nodeClickEvent, Nothing) Then
                nodeClickEvent.Invoke(Me, e)
            End If
        End Sub

        Public Function RaiseClientAPICallbackEvent(ByVal eventArgument As String) As String Implements IClientAPICallbackEventHandler.RaiseClientAPICallbackEvent
            Dim populateOnDemandEvent As DNNTextSuggestEventHandler = Me.PopulateOnDemandEvent
            If Not Object.ReferenceEquals(populateOnDemandEvent, Nothing) Then
                populateOnDemandEvent.Invoke(Me, New DNNTextSuggestEventArgs(Me.DNNNodes, eventArgument))
            End If
            Return Me.DNNNodes.ToJSON
        End Function

        Public Overridable Sub RaisePostBackEvent(ByVal eventArgument As String) Implements IPostBackEventHandler.RaisePostBackEvent
            Dim strArray As String() = Strings.Split(eventArgument, ClientAPI.COLUMN_DELIMITER, -1, CompareMethod.Binary)
            If ((strArray.Length > 1) AndAlso (strArray(1) = "Click")) Then
                Dim e As New DNNTextSuggestEventArgs(Me.DNNNodes, strArray(0))
                Me.OnNodeClick(e)
            End If
        End Sub

        Public Sub RegisterClientScript()
            If Not Me.IsDownLevel Then
                ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_dom)
                ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_dom_positioning)
                ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_xmlhttp)
                WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.js", FileOrder.Js.DnnControlsLabelEdit)
                WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.dnntextsuggest.js")
            End If
        End Sub

        Protected Overrides Sub Render(ByVal writer As HtmlTextWriter)
            MyBase.Render(writer)
            If Not Me.IsDownLevel Then
                Dim properties As Hashtable = Me.MarshalledProperties
                Dim marshalledPropertyJSON As String = ScriptGenerator.GetMarshalledPropertyJSON(Me, properties)
                BaseWebControl.RegisterInitialize(Me, "initTextSuggest", marshalledPropertyJSON)
            End If
        End Sub


        ' Properties
        <DefaultValue(""), ClientProperty, Bindable(True), ClientPropertyName("bid")> _
        Public Property BehaviorID As String
            Get
                Return Conversions.ToString(Me.ViewState("BehaviorID"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("BehaviorID") = Value
            End Set
        End Property

        <DefaultValue(False), Description("Allows developer to force the rendering of the Menu in DownLevel mode"), Category("Behavior")> _
        Public Property ForceDownLevel As Boolean
            Get
                Return Conversions.ToBoolean(Me.ViewState("ForceDownLevel"))
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("ForceDownLevel") = Value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property IsDownLevel As Boolean
            Get
                Return (Me.ForceDownLevel OrElse Not ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML))
            End Get
        End Property

        <Description("Location of ClientAPI js files"), Category("Paths"), DefaultValue("")> _
        Public Property ClientAPIScriptPath As String
            Get
                Return ClientAPI.ScriptPath
            End Get
            Set(ByVal Value As String)
                ClientAPI.ScriptPath = Value
            End Set
        End Property

        <Category("Paths"), Description("Location of dnn.controls.DNNTextSuggest.js file"), DefaultValue("")> _
        Public Property TextSuggestScriptPath As String
            Get
                Return If((Strings.Len(Me.ViewState("TextSuggestScriptPath")) <> 0), Conversions.ToString(Me.ViewState("TextSuggestScriptPath")), Me.ClientAPIScriptPath)
            End Get
            Set(ByVal Value As String)
                Me.ViewState("TextSuggestScriptPath") = Value
            End Set
        End Property

        <ClientProperty, Description("Directory to find the images for the TextSuggest."), ClientPropertyName("sysimgpath"), DefaultValue("images/"), Category("Paths")> _
        Public Property SystemImagesPath As String
            Get
                Return If((Strings.Len(Me.ViewState("SystemImagesPath")) <> 0), Conversions.ToString(Me.ViewState("SystemImagesPath")), "images/")
            End Get
            Set(ByVal Value As String)
                Me.ViewState("SystemImagesPath") = Value
            End Set
        End Property

        Public ReadOnly Property DNNNodes As DNNNodeCollection
            Get
                If Object.ReferenceEquals(Me.m_objNodes, Nothing) Then
                    Me.m_objNodes = New DNNNodeCollection(Me.ClientID)
                End If
                Return Me.m_objNodes
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property SelectedNodes As DNNNodeCollection
            Get
                Dim nodes As New DNNNodeCollection("")
                If Me.Page.IsPostBack Then
                    Dim separator As Char() = New Char() { Me.Delimiter }
                    Dim strArray2 As String() = Me.Text.Split(separator)
                    Dim index As Integer = 0
                    Do While True
                        If (index >= strArray2.Length) Then
                            Exit Do
                        End If
                        Dim expression As String = strArray2(index)
                        If (Strings.Len(expression) > 0) Then
                            Dim strText As String = expression
                            Dim str As String = ""
                            Select Case Me.IDToken
                                Case eIDTokenChar.Brackets
                                    Dim length As Integer = expression.LastIndexOf(" [", StringComparison.Ordinal)
                                    Dim startIndex As Integer = (length + " [".Length)
                                    Dim num2 As Integer = expression.LastIndexOf("]", StringComparison.Ordinal)
                                    If (If(((length <= -1) OrElse (num2 <= startIndex)), 0, 1) <> 0) Then
                                        strText = expression.Substring(0, length)
                                        str = expression.Substring(startIndex, (num2 - startIndex))
                                    End If
                                    Exit Select
                                Case Else
                                    Exit Select
                            End Select
                            Dim objNode As New DNNNode(strText) With { _
                                .Key = str _
                            }
                            nodes.Add(objNode)
                        End If
                        index += 1
                    Loop
                End If
                Return nodes
            End Get
        End Property

        <ClientPropertyName("target"), Description("If ClickAction for a node is set to navigate this is the target frame that will do the navigating."), Category("Behavior"), ClientProperty, DefaultValue("")> _
        Public Property Target As String
            Get
                Return Conversions.ToString(Me.ViewState("Target"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("Target") = Value
            End Set
        End Property

        <ClientProperty, Description("Default Classname for node."), Category("Appearance"), ClientPropertyName("css"), DefaultValue("")> _
        Public Property DefaultNodeCssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("DefaultNodeCssClass"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("DefaultNodeCssClass") = Value
            End Set
        End Property

        <Category("Appearance"), ClientPropertyName("csschild"), ClientProperty, DefaultValue(""), Description("Default Classname for child node.")> _
        Public Property DefaultChildNodeCssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("DefaultChildNodeCssClass"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("DefaultChildNodeCssClass") = Value
            End Set
        End Property

        <Category("Appearance"), DefaultValue(""), Description("Default Classname for node when hovered."), ClientPropertyName("csshover"), ClientProperty> _
        Public Property DefaultNodeCssClassOver As String
            Get
                Return Conversions.ToString(Me.ViewState("DefaultNodeCssClassOver"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("DefaultNodeCssClassOver") = Value
            End Set
        End Property

        <Description("Default Classname for node when selected."), Category("Appearance"), DefaultValue(""), ClientProperty, ClientPropertyName("csssel")> _
        Public Property DefaultNodeCssClassSelected As String
            Get
                Return Conversions.ToString(Me.ViewState("DefaultNodeCssClassSelected"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("DefaultNodeCssClassSelected") = Value
            End Set
        End Property

        <Description("Default Classname container holding all of suggestion nodes."), Category("Appearance"), ClientProperty, DefaultValue(""), ClientPropertyName("tscss")> _
        Public Property TextSuggestCssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("TextSuggestCssClass"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("TextSuggestCssClass") = Value
            End Set
        End Property

        <DefaultValue(""), ClientProperty, ClientPropertyName("js"), Description("Allows you to have a common JS function be invoked for all nodes, unless a different JS function is provided on the node level."), Category("Behavior")> _
        Public Property JSFunction As String
            Get
                Return Conversions.ToString(Me.ViewState("JSFunction"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("JSFunction") = Value
            End Set
        End Property

        <ClientProperty, Category("Behavior"), ClientPropertyName("callbackSF"), DefaultValue(""), Description("If callbacks are supported/enabled, this javascript function will be invoked with each status change of the xmlhttp request.")> _
        Public Property CallbackStatusFunction As String
            Get
                Return Conversions.ToString(Me.ViewState("CallbackStatusFunction"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("CallbackStatusFunction") = Value
            End Set
        End Property

        <ClientPropertyName("del"), DefaultValue(""), Description("Specifies a delimiter to be used to allow for multiple entries to be added."), Category("Behavior"), ClientProperty> _
        Public Property Delimiter As Char
            Get
                Return Conversions.ToChar(Me.ViewState("Delimiter"))
            End Get
            Set(ByVal Value As Char)
                Me.ViewState("Delimiter") = Value
            End Set
        End Property

        <Category("Behavior"), DefaultValue(0), Description("Specifies a type of character to be used to surround/delimit the underlying value/id of the selected item.")> _
        Public Property IDToken As eIDTokenChar
            Get
                Return DirectCast(Conversions.ToInteger(Me.ViewState("IDToken")), eIDTokenChar)
            End Get
            Set(ByVal Value As eIDTokenChar)
                Me.ViewState("IDToken") = CInt(Value)
            End Set
        End Property

        <Description("Minimum number of characters typed before a lookup will be invoked"), ClientProperty, DefaultValue(1), ClientPropertyName("minChar"), Category("Behavior")> _
        Public Property MinCharacterLookup As Integer
            Get
                Return If((Strings.Len(Me.ViewState("MinCharacterLookup")) <> 0), Conversions.ToInteger(Me.ViewState("MinCharacterLookup")), 1)
            End Get
            Set(ByVal Value As Integer)
                Me.ViewState("MinCharacterLookup") = Value
            End Set
        End Property

        <Category("Behavior"), Description("Maximum number of rows to display."), ClientPropertyName("maxRows"), ClientProperty, DefaultValue(10)> _
        Public Property MaxSuggestRows As Integer
            Get
                Return If((Strings.Len(Me.ViewState("MaxSuggestRows")) <> 0), Conversions.ToInteger(Me.ViewState("MaxSuggestRows")), 10)
            End Get
            Set(ByVal Value As Integer)
                Me.ViewState("MaxSuggestRows") = Value
            End Set
        End Property

        <DefaultValue(500), Description("Number of milliseconds to wait after keypress before a lookup takes place."), ClientProperty, ClientPropertyName("ludelay"), Category("Behavior")> _
        Public Property LookupDelay As Integer
            Get
                Return If((Strings.Len(Me.ViewState("LookupDelay")) <> 0), Conversions.ToInteger(Me.ViewState("LookupDelay")), 500)
            End Get
            Set(ByVal Value As Integer)
                Me.ViewState("LookupDelay") = Value
            End Set
        End Property

        <Description("Number of milliseconds to wait after focus is lost before a hiding the results."), DefaultValue(500), Category("Behavior"), ClientPropertyName("lfdelay"), ClientProperty> _
        Public Property LostFocusDelay As Integer
            Get
                Return If((Strings.Len(Me.ViewState("LostFocusDelay")) <> 0), Conversions.ToInteger(Me.ViewState("LostFocusDelay")), 500)
            End Get
            Set(ByVal Value As Integer)
                Me.ViewState("LostFocusDelay") = Value
            End Set
        End Property

        <DefaultValue(False), ClientPropertyName("casesens"), Description("Determines if lookup uses a case sensitve match."), Category("Behavior"), ClientProperty> _
        Public Property CaseSensitive As Boolean
            Get
                Return If((Strings.Len(Me.ViewState("CaseSensitive")) <> 0), Conversions.ToBoolean(Me.ViewState("CaseSensitive")), False)
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("CaseSensitive") = Value
            End Set
        End Property

        <DefaultValue(0), Description("Image to display during a callback"), Bindable(True), Category("Behavior")> _
        Public Property CallBackType As ClientAPICallBackResponse.CallBackTypeCode
            Get
                Return If((Strings.Len(Me.ViewState("CallBackTypeCode")) <= 0), ClientAPICallBackResponse.CallBackTypeCode.Simple, DirectCast(Conversions.ToInteger(Me.ViewState("CallBackTypeCode")), ClientAPICallBackResponse.CallBackTypeCode))
            End Get
            Set(ByVal Value As ClientAPICallBackResponse.CallBackTypeCode)
                Me.ViewState("CallBackTypeCode") = Value
            End Set
        End Property

        <Bindable(True), Category("Behavior"), Description("This property allows for data on page to be posted in callback")> _
        Public Property CallBackPostChildrenOf As String
            Get
                Return If((Strings.Len(Me.ViewState("CallBackPostChildrenOf")) <= 0), "", Conversions.ToString(Me.ViewState("CallBackPostChildrenOf")))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("CallBackPostChildrenOf") = Value
            End Set
        End Property


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private m_objNodes As DNNNodeCollection

        ' Nested Types
        Public Delegate Sub DNNDNNNodeClickHandler(ByVal source As Object, ByVal e As DNNTextSuggestEventArgs)

        Public Delegate Sub DNNTextSuggestEventHandler(ByVal source As Object, ByVal e As DNNTextSuggestEventArgs)

        Public Enum eIDTokenChar
            ' Fields
            None = 0
            Brackets = 1
            Paranthesis = 2
        End Enum
    End Class
End Namespace

