' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Utilities
Imports DotNetNuke.UI.Utilities.Animation
Imports DotNetNuke.Web.Client
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    <Designer(GetType(DNNMenuDesigner))> _
    Public Class DNNMenu
        Inherits WebControl
        Implements IPostBackEventHandler, IPostBackDataHandler, IClientAPICallbackEventHandler
        ' Events
        Public Event NodeClick As DNNMenuNodeClickHandler
        Public Event PopulateOnDemand As DNNMenuEventHandler

        ' Methods
        Public Sub New()
            AddHandler MyBase.PreRender, New EventHandler(AddressOf Me.DNNMenu_PreRender)
            DNNMenu.__ENCAddToList(Me)
            Me.m_DNNAnimation = New DNNAnimation
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
            SyncLock DNNMenu.__ENCList
                If (DNNMenu.__ENCList.Count = DNNMenu.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNMenu.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNMenu.__ENCList.RemoveRange(index, (DNNMenu.__ENCList.Count - index))
                            DNNMenu.__ENCList.Capacity = DNNMenu.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNMenu.__ENCList(num2)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNMenu.__ENCList(index) = DNNMenu.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNMenu.__ENCList.Add(New WeakReference(value))
            End SyncLock
        End Sub

        Private Sub DNNMenu_PreRender(ByVal sender As Object, ByVal e As EventArgs)
            Me.RegisterClientScript
            Me.Page.RegisterRequiresPostBack(Me)
            Me.UpdateNodes(Me.MenuNodes)
            If Not Me.IsDownLevel Then
                ClientAPI.RegisterClientVariable(Me.Page, (Me.ClientID & "_json"), ("{" & Me.MenuNodes.ToJSON & "}"), True)
            Else
                Me.ViewState("xml") = Me.MenuNodes.ToXml
            End If
        End Sub

        Protected Overrides Sub Finalize()
            MyBase.Finalize
        End Sub

        Public Function FindNode(ByVal strID As String) As MenuNode
            Return Me.MenuNodes.FindNode(strID)
        End Function

        Public Function FindNodeByKey(ByVal strKey As String) As MenuNode
            Return Me.MenuNodes.FindNodeByKey(strKey)
        End Function

        Public Sub LoadJSON(ByVal dict As Dictionary(Of String, Object))
            Me.m_Nodes = New MenuNodeCollection(dict, Me)
        End Sub

        Public Function LoadPostData(ByVal postDataKey As String, ByVal postCollection As NameValueCollection) As Boolean Implements IPostBackDataHandler.LoadPostData
            Dim flag As Boolean
            Return flag
        End Function

        Private Sub LoadPostedJSON()
            Dim clientVariable As String = ""
            If Not Me.IsDownLevel Then
                clientVariable = ClientAPI.GetClientVariable(Me.Page, (Me.ClientID & "_json"))
            End If
            If Not String.IsNullOrEmpty(clientVariable) Then
                Dim dict As Dictionary(Of String, Object) = MSAJAX.Deserialize(Of Dictionary(Of String, Object))(clientVariable)
                If dict.ContainsKey("nodes") Then
                    Me.LoadJSON(dict)
                End If
            End If
        End Sub

        Private Sub LoadPostedXML()
            Dim expression As String = ""
            If Not Me.IsDownLevel Then
                expression = ClientAPI.GetClientVariable(Me.Page, (Me.ClientID & "_xml"))
            End If
            If (Strings.Len(expression) > 0) Then
                Me.LoadXml(expression)
            End If
        End Sub

        Protected Overrides Sub LoadViewState(ByVal state As Object)
            If (Not state Is Nothing) Then
                Dim objArray As Object() = DirectCast(state, Object())
                If (Not objArray(0) Is Nothing) Then
                    MyBase.LoadViewState(objArray(0))
                End If
                If (Not objArray(1) Is Nothing) Then
                    DirectCast(Me.ImageList, IStateManager).LoadViewState(objArray(1))
                End If
            End If
        End Sub

        Public Sub LoadXml(ByVal strXml As String)
            Me.m_Nodes = New MenuNodeCollection(strXml, "", Me)
        End Sub

        Private Function MarshalledProperties() As Hashtable
            Dim hashtable2 As New Hashtable
            Dim flag As Boolean = (Me.ImageList.Count > 0)
            If flag Then
                Dim enumerator As IEnumerator = Nothing
                Dim list As New SortedList
                Dim expression As String = ""
                Dim str As String = ""
                Try 
                    enumerator = Me.ImageList.GetEnumerator
                    Do While True
                        flag = enumerator.MoveNext
                        If Not flag Then
                            Exit Do
                        End If
                        Dim current As NodeImage = DirectCast(enumerator.Current, NodeImage)
                        If (current.ImageUrl.IndexOf("/", StringComparison.Ordinal) > -1) Then
                            Dim str4 As String = current.ImageUrl.Substring(0, (current.ImageUrl.LastIndexOf("/", StringComparison.Ordinal) + 1))
                            Dim str3 As String = current.ImageUrl.Substring((current.ImageUrl.LastIndexOf("/", StringComparison.Ordinal) + 1))
                            If Not list.ContainsValue(str4) Then
                                list.Add(list.Count, str4)
                            End If
                            current.ImageUrl = $"[{list.IndexOfValue(str4).ToString(CultureInfo.InvariantCulture)}]{str3}"
                        End If
                        expression = (expression & Conversions.ToString(Interaction.IIf((Strings.Len(expression) > 0), ",", "")) & current.ImageUrl)
                    Loop
                Finally
                    If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                        TryCast(enumerator,IDisposable).Dispose
                    End If
                End Try
                Dim num3 As Integer = (list.Count - 1)
                Dim index As Integer = 0
                Do While True
                    Dim num4 As Integer = num3
                    If (index > num4) Then
                        hashtable2.Add("imagelist", expression)
                        hashtable2.Add("imagepaths", str)
                        Exit Do
                    End If
                    str = (str & Conversions.ToString(Interaction.IIf((Strings.Len(str) > 0), ",", "")) & Conversions.ToString(list.GetByIndex(index)))
                    index += 1
                Loop
            End If
            hashtable2.Add("postback", ClientAPI.GetPostBackEventReference(Me, ("[NODEID]" & ClientAPI.COLUMN_DELIMITER & "Click")))
            If Me.PopulateNodesFromClient Then
                If ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.XMLHTTP) Then
                    hashtable2.Add("callback", ClientAPI.GetCallbackEventReference(Me, "'[NODEXML]'", "this.callBackSuccess", "mNode", "this.callBackFail", "this.callBackStatus"))
                Else
                    hashtable2.Add("callback", ClientAPI.GetPostBackClientHyperlink(Me, ("[NODEID]" & ClientAPI.COLUMN_DELIMITER & "OnDemand")))
                End If
                If (Strings.Len(Me.CallbackStatusFunction) > 0) Then
                    hashtable2.Add("callbacksf", Me.CallbackStatusFunction)
                End If
            End If
            Return hashtable2
        End Function

        Protected Overrides Sub OnInit(ByVal e As EventArgs)
            MyBase.OnInit(e)
            If ClientAPI.NeedsDNNVariable(Me) Then
                AddHandler Me.Page.Load, New EventHandler(AddressOf Me.ParentOnLoad)
            Else
                Me.LoadPostedJSON
            End If
        End Sub

        Protected Overrides Sub OnLoad(ByVal e As EventArgs)
            If (If((Not Me.IsDownLevel OrElse (Strings.Len(Me.ViewState("xml")) <= 0)), 0, 1) <> 0) Then
                Me.m_Nodes = New MenuNodeCollection(Conversions.ToString(Me.ViewState("xml")), "", Me)
            End If
        End Sub

        Public Overridable Sub OnNodeClick(ByVal e As DNNMenuNodeClickEventArgs)
            Dim nodeClickEvent As DNNMenuNodeClickHandler = Me.NodeClickEvent
            If Not Object.ReferenceEquals(nodeClickEvent, Nothing) Then
                nodeClickEvent.Invoke(Me, e)
            End If
        End Sub

        Protected Sub ParentOnLoad(ByVal Sender As Object, ByVal e As EventArgs)
            ClientAPI.RegisterDNNVariableControl(Me)
            Me.LoadPostedJSON
        End Sub

        Public Function RaiseClientAPICallbackEvent(ByVal eventArgument As String) As String Implements IClientAPICallbackEventHandler.RaiseClientAPICallbackEvent
            Dim str As String
            Dim strArray As String() = Strings.Split(eventArgument, ClientAPI.COLUMN_DELIMITER, -1, CompareMethod.Binary)
            Dim collection =  New MenuNodeCollection(("<root>" & strArray(0) & "</root>"), "", Me)
            Dim objA As MenuNode = collection(0)
            If Object.ReferenceEquals(objA, Nothing) Then
                str = Nothing
            Else
                Dim populateOnDemandEvent As DNNMenuEventHandler = Me.PopulateOnDemandEvent
                If Not Object.ReferenceEquals(populateOnDemandEvent, Nothing) Then
                    populateOnDemandEvent.Invoke(Me, New DNNMenuEventArgs(objA))
                End If
                Dim node2 As MenuNode = Me.FindNode(objA.ID)
                str = If(Object.ReferenceEquals(node2, Nothing), objA.MenuNodes.ToJSON, node2.MenuNodes.ToJSON)
            End If
            Return str
        End Function

        Public Overridable Sub RaisePostBackEvent(ByVal eventArgument As String) Implements IPostBackEventHandler.RaisePostBackEvent
            Dim strArray As String() = Strings.Split(eventArgument, ClientAPI.COLUMN_DELIMITER, -1, CompareMethod.Binary)
            Dim objA As MenuNode = Me.MenuNodes.FindNode(strArray(0))
            If Not Object.ReferenceEquals(objA, Nothing) Then
                If (strArray.Length <= 1) Then
                    objA.Click
                Else
                    Dim str As String = strArray(1)
                    If (str = "Click") Then
                        objA.Click
                    ElseIf (str = "Checked") Then
                        objA.Selected = Not objA.Selected
                    ElseIf (str = "OnDemand") Then
                        Dim populateOnDemandEvent As DNNMenuEventHandler = Me.PopulateOnDemandEvent
                        If Not Object.ReferenceEquals(populateOnDemandEvent, Nothing) Then
                            populateOnDemandEvent.Invoke(Me, New DNNMenuEventArgs(objA))
                        End If
                    End If
                End If
            End If
        End Sub

        Public Sub RaisePostDataChangedEvent() Implements IPostBackDataHandler.RaisePostDataChangedEvent
        End Sub

        Public Sub RegisterClientScript()
            If Not Me.IsDownLevel Then
                ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_dom)
                ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_dom_positioning)
                If Me.PopulateNodesFromClient Then
                    ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_xmlhttp)
                End If
                If (Me.Animation.AnimationType <> AnimationType.None) Then
                    ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_motion)
                End If
                WebControls.RegisterSubmitComponent(Me.Page)
                WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.js", FileOrder.Js.DnnControlsLabelEdit)
                WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.dnnmenu.js")
            End If
        End Sub

        <Obsolete("RegisterDNNVariableControl on control level is now obsolete.  Use RegisterDNNVariableControl.WebControls")> _
        Public Sub RegisterDNNVariableControl()
            ClientAPI.RegisterDNNVariableControl(Me)
        End Sub

        Protected Overrides Sub Render(ByVal writer As HtmlTextWriter)
            Dim enumerator As IEnumerator = Nothing
            Me.MenuWriter.RenderMenu(writer, Me)
            Try 
                enumerator = Me.Controls.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    DirectCast(enumerator.Current, Control).RenderControl(writer)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            If Not Me.IsDownLevel Then
                Dim properties As Hashtable = Me.MarshalledProperties
                ScriptGenerator.GetMarshalledPropertyJSON(Me.m_DNNAnimation, properties)
                Dim marshalledPropertyJSON As String = ScriptGenerator.GetMarshalledPropertyJSON(Me, properties)
                BaseWebControl.RegisterInitialize(Me, "initMenu", marshalledPropertyJSON)
            End If
        End Sub

        Protected Overrides Function SaveViewState() As Object
            Return New Object() { MyBase.SaveViewState, DirectCast(Me.ImageList, IStateManager).SaveViewState }
        End Function

        Protected Overrides Sub TrackViewState()
            MyBase.TrackViewState
        End Sub

        Private Sub UpdateNodes(ByVal objNodes As MenuNodeCollection)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = objNodes.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As MenuNode = DirectCast(enumerator.Current, MenuNode)
                    If (Strings.Len(current.Image) > 0) Then
                        current.ImageIndex = If(Me.ImageList.Contains(New NodeImage(current.Image)), Me.ImageList.IndexOf(New NodeImage(current.Image)), Me.ImageList.Add(current.Image))
                        current.Image = Nothing
                    End If
                    If (current.DNNNodes.Count > 0) Then
                        Me.UpdateNodes(current.MenuNodes)
                    End If
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
        End Sub


        ' Properties
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
                Return (Me.ForceDownLevel OrElse ((Me.RenderMode = MenuRenderMode.DownLevel) OrElse Not ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML)))
            End Get
        End Property

        Public Property IsCrawler As Boolean
            Get
                Return If((Strings.Len(Me.ViewState("IsCrawler")) <> 0), Conversions.ToBoolean(Me.ViewState("IsCrawler")), HttpContext.Current.Request.Browser.Crawler)
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("IsCrawler") = Value
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

        Public Property MenuScriptPath As String
            Get
                Return If((Strings.Len(Me.ViewState("MenuScriptPath")) <> 0), Conversions.ToString(Me.ViewState("MenuScriptPath")), Me.ClientAPIScriptPath)
            End Get
            Set(ByVal Value As String)
                Me.ViewState("MenuScriptPath") = Value
            End Set
        End Property

        <ClientPropertyName("sysimgpath"), UrlProperty, DefaultValue("images/"), ClientProperty, Description("Directory to find the images for the menu.  Need to have spacer.gif here!")> _
        Public Property SystemImagesPath As String
            Get
                Return If((Strings.Len(Me.ViewState("SystemImagesPath")) <> 0), Conversions.ToString(Me.ViewState("SystemImagesPath")), "images/")
            End Get
            Set(ByVal Value As String)
                Me.ViewState("SystemImagesPath") = Value
            End Set
        End Property

        <PersistenceMode(PersistenceMode.InnerProperty), Browsable(True)> _
        Public ReadOnly Property MenuNodes As MenuNodeCollection
            Get
                If Object.ReferenceEquals(Me.m_Nodes, Nothing) Then
                    Me.m_Nodes = New MenuNodeCollection(Me.ClientID, Me)
                End If
                Return Me.m_Nodes
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property SelectedMenuNodes As Collection
            Get
                Return Me.MenuNodes.FindSelectedNodes
            End Get
        End Property

        <Bindable(True), ClientProperty, DefaultValue(""), UrlProperty, PersistenceMode(PersistenceMode.Attribute), ClientPropertyName("rarrowimg")> _
        Public Property RootArrowImage As String
            Get
                Return Conversions.ToString(Me.ViewState("RootArrowImage"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("RootArrowImage") = Value
            End Set
        End Property

        <DefaultValue(""), PersistenceMode(PersistenceMode.Attribute), UrlProperty, Bindable(True), ClientPropertyName("carrowimg"), ClientProperty> _
        Public Property ChildArrowImage As String
            Get
                Return Conversions.ToString(Me.ViewState("ChildArrowImage"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("ChildArrowImage") = Value
            End Set
        End Property

        <ClientPropertyName("workimg"), ClientProperty, PersistenceMode(PersistenceMode.Attribute), DefaultValue("dnnanim.gif"), Bindable(True)> _
        Public Property WorkImage As String
            Get
                Return Conversions.ToString(Me.ViewState("WorkImage"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("WorkImage") = Value
            End Set
        End Property

        <DefaultValue(""), Bindable(True), ClientPropertyName("bid"), ClientProperty> _
        Public Property BehaviorID As String
            Get
                Return Conversions.ToString(Me.ViewState("BehaviorID"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("BehaviorID") = Value
            End Set
        End Property

        <DefaultValue(""), Bindable(True), ClientPropertyName("target"), ClientProperty> _
        Public Property Target As String
            Get
                Return Conversions.ToString(Me.ViewState("Target"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("Target") = Value
            End Set
        End Property

        <PersistenceMode(PersistenceMode.InnerProperty), Browsable(True)> _
        Public ReadOnly Property ImageList As NodeImageCollection
            Get
                If Object.ReferenceEquals(Me.m_Images, Nothing) Then
                    Me.m_Images = New NodeImageCollection
                    If Me.IsTrackingViewState Then
                        DirectCast(Me.m_Images, IStateManager).TrackViewState
                    End If
                End If
                Return Me.m_Images
            End Get
        End Property

        <ClientPropertyName("css"), DefaultValue(""), Bindable(True), ClientProperty, PersistenceMode(PersistenceMode.Attribute)> _
        Public Property DefaultNodeCssClass As String
            Get
                Return Me.CssClass
            End Get
            Set(ByVal Value As String)
                Me.CssClass = Value
            End Set
        End Property

        <ClientProperty, ClientPropertyName("csschild"), DefaultValue(""), Bindable(True), PersistenceMode(PersistenceMode.Attribute)> _
        Public Property DefaultChildNodeCssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("DefaultChildNodeCssClass"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("DefaultChildNodeCssClass") = Value
            End Set
        End Property

        <ClientPropertyName("csshover"), ClientProperty, PersistenceMode(PersistenceMode.Attribute), DefaultValue(""), Bindable(True)> _
        Public Property DefaultNodeCssClassOver As String
            Get
                Return Conversions.ToString(Me.ViewState("DefaultNodeCssClassOver"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("DefaultNodeCssClassOver") = Value
            End Set
        End Property

        <Bindable(True), PersistenceMode(PersistenceMode.Attribute), DefaultValue(""), ClientPropertyName("csssel"), ClientProperty> _
        Public Property DefaultNodeCssClassSelected As String
            Get
                Return Conversions.ToString(Me.ViewState("DefaultNodeCssClassSelected"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("DefaultNodeCssClassSelected") = Value
            End Set
        End Property

        <Bindable(True), DefaultValue(""), ClientPropertyName("cssicon"), ClientProperty, PersistenceMode(PersistenceMode.Attribute)> _
        Public Property DefaultIconCssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("DefaultIconCssClass"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("DefaultIconCssClass") = Value
            End Set
        End Property

        <PersistenceMode(PersistenceMode.Attribute), ClientProperty, ClientPropertyName("mbcss"), DefaultValue(""), Bindable(True)> _
        Public Property MenuBarCssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("MenuBarCssClass"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("MenuBarCssClass") = Value
            End Set
        End Property

        <ClientPropertyName("mcss"), ClientProperty, PersistenceMode(PersistenceMode.Attribute), DefaultValue(""), Bindable(True)> _
        Public Property MenuCssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("MenuCssClass"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("MenuCssClass") = Value
            End Set
        End Property

        Private ReadOnly Property MenuWriter As IDNNMenuWriter
            Get
                If Object.ReferenceEquals(Me.m_Writer, Nothing) Then
                    If Me.IsDownLevel Then
                        Me.RenderMode = MenuRenderMode.UnorderedList
                    ElseIf Me.IsCrawler Then
                        Me.RenderMode = MenuRenderMode.UnorderedList
                    End If
                    Select Case Me.RenderMode
                        Case MenuRenderMode.Normal, MenuRenderMode.NoTables
                            Me.m_Writer = New DNNMenuUpLevelWriter
                            Exit Select
                        Case MenuRenderMode.UnorderedList
                            Me.m_Writer = New DNNMenuListWriter
                            Exit Select
                        Case MenuRenderMode.DownLevel
                            Me.m_Writer = New DNNMenuWriter(Me.IsCrawler)
                            Exit Select
                        Case Else
                            Exit Select
                    End Select
                End If
                Return Me.m_Writer
            End Get
        End Property

        <ClientProperty, ClientPropertyName("js"), DefaultValue("")> _
        Public Property JSFunction As String
            Get
                Return Conversions.ToString(Me.ViewState("JSFunction"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("JSFunction") = Value
            End Set
        End Property

        <DefaultValue(True), Bindable(False), PersistenceMode(PersistenceMode.Attribute)> _
        Public Property PopulateNodesFromClient As Boolean
            Get
                Dim flag As Boolean
                Select Case Me.RenderMode
                    Case MenuRenderMode.UnorderedList, MenuRenderMode.DownLevel
                        flag = False
                        Exit Select
                    Case Else
                        flag = If((Not Me.ViewState("PopNodesFromClient") Is Nothing), Conversions.ToBoolean(Me.ViewState("PopNodesFromClient")), True)
                        Exit Select
                End Select
                Return flag
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("PopNodesFromClient") = Value
            End Set
        End Property

        <Bindable(True), PersistenceMode(PersistenceMode.Attribute)> _
        Public Property CallbackStatusFunction As String
            Get
                Return Conversions.ToString(Me.ViewState("CallbackStatusFunction"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("CallbackStatusFunction") = Value
            End Set
        End Property

        <Bindable(True), DefaultValue(0), ClientPropertyName("orient"), ClientProperty, PersistenceMode(PersistenceMode.Attribute)> _
        Public Property Orientation As Orientation
            Get
                Return If((Strings.Len(Me.ViewState("Orientation")) <= 0), Orientation.Horizontal, DirectCast(Conversions.ToInteger(Me.ViewState("Orientation")), Orientation))
            End Get
            Set(ByVal Value As Orientation)
                Me.ViewState("Orientation") = Value
            End Set
        End Property

        <PersistenceMode(PersistenceMode.Attribute), DefaultValue(1), Bindable(True), ClientProperty, ClientPropertyName("suborient")> _
        Public Property SubMenuOrientation As Orientation
            Get
                Return If((Strings.Len(Me.ViewState("SubMenuOrientation")) <= 0), Orientation.Vertical, DirectCast(Conversions.ToInteger(Me.ViewState("SubMenuOrientation")), Orientation))
            End Get
            Set(ByVal Value As Orientation)
                Me.ViewState("SubMenuOrientation") = Value
            End Set
        End Property

        <DefaultValue(0), ClientPropertyName("rmode"), ClientProperty> _
        Public Property RenderMode As MenuRenderMode
            Get
                Return If((Strings.Len(Conversions.ToString(Me.ViewState("RenderMode"))) <> 0), DirectCast(Conversions.ToInteger(Me.ViewState("RenderMode")), MenuRenderMode), MenuRenderMode.Normal)
            End Get
            Set(ByVal Value As MenuRenderMode)
                Me.ViewState("RenderMode") = Value
            End Set
        End Property

        <PersistenceMode(PersistenceMode.Attribute), Obsolete("Use RenderMode"), DefaultValue(True)> _
        Public Property UseTables As Boolean
            Get
                Dim flag As Boolean
                Me.RenderMode = MenuRenderMode.Normal
                Return flag
            End Get
            Set(ByVal Value As Boolean)
                Me.RenderMode = If(Not Value, MenuRenderMode.NoTables, MenuRenderMode.Normal)
            End Set
        End Property

        <Description("Number of milliseconds to wait befor hiding sub-menu on mouse out"), Category("Behavior"), ClientPropertyName("moutdelay"), ClientProperty, DefaultValue(500), PersistenceMode(PersistenceMode.Attribute)> _
        Public Property MouseOutDelay As Integer
            Get
                Return If((Strings.Len(Conversions.ToString(Me.ViewState("MouseOutDelay"))) <> 0), Conversions.ToInteger(Me.ViewState("MouseOutDelay")), 500)
            End Get
            Set(ByVal Value As Integer)
                Me.ViewState("MouseOutDelay") = Value
            End Set
        End Property

        <ClientPropertyName("mindelay"), PersistenceMode(PersistenceMode.Attribute), Category("Behavior"), ClientProperty, DefaultValue(250), Description("Number of milliseconds to wait befor displaying sub-menu on mouse over")> _
        Public Property MouseInDelay As Integer
            Get
                Return If((Strings.Len(Conversions.ToString(Me.ViewState("MouseInDelay"))) <> 0), Conversions.ToInteger(Me.ViewState("MouseInDelay")), 250)
            End Get
            Set(ByVal Value As Integer)
                Me.ViewState("MouseInDelay") = Value
            End Set
        End Property

        <ClientProperty, DefaultValue(False), ClientPropertyName("enablepbstate"), PersistenceMode(PersistenceMode.Attribute)> _
        Public Property EnablePostbackState As Boolean
            Get
                Return If((Strings.Len(Conversions.ToString(Me.ViewState("EnablePostbackState"))) <> 0), Conversions.ToBoolean(Me.ViewState("EnablePostbackState")), False)
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("EnablePostbackState") = Value
            End Set
        End Property

        <NotifyParentProperty(True), Description("The animation to be applied to showing/hiding the menus"), Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)> _
        Public ReadOnly Property Animation As DNNAnimation
            Get
                Return Me.m_DNNAnimation
            End Get
        End Property


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private m_Nodes As MenuNodeCollection
        Private m_Images As NodeImageCollection
        Private m_Writer As IDNNMenuWriter
        Private m_DNNAnimation As DNNAnimation

        ' Nested Types
        Public Delegate Sub DNNMenuEventHandler(ByVal source As Object, ByVal e As DNNMenuEventArgs)

        Public Delegate Sub DNNMenuNodeClickHandler(ByVal source As Object, ByVal e As DNNMenuNodeClickEventArgs)

        Public Enum MenuRenderMode
            ' Fields
            Normal = 0
            NoTables = 1
            UnorderedList = 2
            DownLevel = 3
        End Enum
    End Class
End Namespace

