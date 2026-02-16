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
Imports System.Runtime.CompilerServices
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    <Designer(GetType(DNNTreeDesigner)), ControlBuilder(GetType(DNNTreeBuilder)), ToolboxData("<{0}:DNNTree runat=server></{0}:DNNTree>"), DefaultProperty("Nodes")> _
    Public Class DnnTree
        Inherits WebControl
        Implements IPostBackEventHandler, IPostBackDataHandler, IClientAPICallbackEventHandler
        ' Events
        Public Event Collapse As DNNTreeEventHandler
        Public Event Expand As DNNTreeEventHandler
        Public Event NodeClick As DNNTreeNodeClickHandler
        Public Event PopulateOnDemand As DNNTreeEventHandler

        ' Methods
        Public Sub New()
            AddHandler MyBase.PreRender, New EventHandler(AddressOf Me.DnnTree_PreRender)
            DnnTree.__ENCAddToList(Me)
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
            SyncLock DnnTree.__ENCList
                If (DnnTree.__ENCList.Count = DnnTree.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DnnTree.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DnnTree.__ENCList.RemoveRange(index, (DnnTree.__ENCList.Count - index))
                            DnnTree.__ENCList.Capacity = DnnTree.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DnnTree.__ENCList(num2)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DnnTree.__ENCList(index) = DnnTree.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DnnTree.__ENCList.Add(New WeakReference(value))
            End SyncLock
        End Sub

        Public Sub ClearSelections()
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.TreeNodes.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As TreeNode = DirectCast(enumerator.Current, TreeNode)
                    Me.ClearSelections(current)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
        End Sub

        Public Sub ClearSelections(ByVal Parent As TreeNode)
            Dim enumerator As IEnumerator = Nothing
            Parent.Selected = False
            Try 
                enumerator = Parent.TreeNodes.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As TreeNode = DirectCast(enumerator.Current, TreeNode)
                    Me.ClearSelections(current)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
        End Sub

        Private Sub DnnTree_PreRender(ByVal sender As Object, ByVal e As EventArgs)
            Me.RegisterClientScript
            Me.Page.RegisterRequiresPostBack(Me)
            Me.UpdateNodes(Me.TreeNodes)
            If Not Me.IsDownLevel Then
                ClientAPI.RegisterClientVariable(Me.Page, (Me.ClientID & "_json"), ("{" & Me.TreeNodes.ToJSON & "}"), True)
            Else
                Me.ViewState("xml") = Me.TreeNodes.ToXml
                If ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML) Then
                    ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn)
                    If (Me.SelectedTreeNodes.Count > 0) Then
                        ClientAPI.RegisterClientVariable(Me.Page, (Me.ClientID & "_selNode"), DirectCast(Me.SelectedTreeNodes(1), TreeNode).ToJSON(False), True)
                    End If
                End If
            End If
        End Sub

        Public Function FindNode(ByVal strID As String) As TreeNode
            Return Me.TreeNodes.FindNode(strID)
        End Function

        Public Function FindNodeByKey(ByVal strKey As String) As TreeNode
            Return Me.TreeNodes.FindNodeByKey(strKey)
        End Function

        Public Sub LoadJSON(ByVal dict As Dictionary(Of String, Object))
            Me.m_Nodes = New TreeNodeCollection(dict, Me)
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

        Private Sub LoadPostedXml()
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
            If (If((Not Me.IsDownLevel OrElse (Strings.Len(Me.ViewState("xml")) <= 0)), 0, 1) <> 0) Then
                Me.m_Nodes = New TreeNodeCollection(Conversions.ToString(Me.ViewState("xml")), "", Me)
            End If
        End Sub

        Public Sub LoadXml(ByVal strXml As String)
            Me.m_Nodes = New TreeNodeCollection(strXml, "", Me)
        End Sub

        Public Overridable Sub OnCollapse(ByVal e As DNNTreeEventArgs)
            Dim collapseEvent As DNNTreeEventHandler = Me.CollapseEvent
            If Not Object.ReferenceEquals(collapseEvent, Nothing) Then
                collapseEvent.Invoke(Me, e)
            End If
        End Sub

        Public Overridable Sub OnExpand(ByVal e As DNNTreeEventArgs)
            Dim expandEvent As DNNTreeEventHandler = Me.ExpandEvent
            If Not Object.ReferenceEquals(expandEvent, Nothing) Then
                expandEvent.Invoke(Me, e)
            End If
        End Sub

        Protected Overrides Sub OnInit(ByVal e As EventArgs)
            MyBase.OnInit(e)
            If ClientAPI.NeedsDNNVariable(Me) Then
                AddHandler Me.Page.Load, New EventHandler(AddressOf Me.ParentOnLoad)
            Else
                Me.LoadPostedJSON
            End If
        End Sub

        Public Overridable Sub OnNodeClick(ByVal e As DNNTreeNodeClickEventArgs)
            Dim nodeClickEvent As DNNTreeNodeClickHandler = Me.NodeClickEvent
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
            Dim collection = New TreeNodeCollection(("<root>" & strArray(0) & "</root>"), "", Me)
            Dim objA As TreeNode = collection(0)
            If Object.ReferenceEquals(objA, Nothing) Then
                str = Nothing
            Else
                Dim populateOnDemandEvent As DNNTreeEventHandler = Me.PopulateOnDemandEvent
                If Not Object.ReferenceEquals(populateOnDemandEvent, Nothing) Then
                    populateOnDemandEvent.Invoke(Me, New DNNTreeEventArgs(objA))
                End If
                Dim node2 As TreeNode = Me.FindNode(objA.ID)
                str = If(Object.ReferenceEquals(node2, Nothing), objA.TreeNodes.ToJSON, node2.TreeNodes.ToJSON)
            End If
            Return str
        End Function

        Public Overridable Sub RaisePostBackEvent(ByVal eventArgument As String) Implements IPostBackEventHandler.RaisePostBackEvent
            Dim strArray As String() = Strings.Split(eventArgument, ClientAPI.COLUMN_DELIMITER, -1, CompareMethod.Binary)
            Dim objA As TreeNode = Me.TreeNodes.FindNode(strArray(0))
            If Not Object.ReferenceEquals(objA, Nothing) Then
                Dim populateOnDemandEvent As DNNTreeEventHandler
                If (strArray.Length <= 1) Then
                    If objA.IsExpanded Then
                        objA.Collapse
                    Else
                        objA.Expand
                    End If
                    If (If(((objA.DNNNodes.Count <> 0) OrElse Not Me.PopulateNodesFromClient), 0, 1) <> 0) Then
                        populateOnDemandEvent = Me.PopulateOnDemandEvent
                        If Not Object.ReferenceEquals(populateOnDemandEvent, Nothing) Then
                            populateOnDemandEvent.Invoke(Me, New DNNTreeEventArgs(objA))
                        End If
                    End If
                Else
                    Dim str As String = strArray(1)
                    If (str <> "Click") Then
                        If (str = "Checked") Then
                            objA.Selected = Not objA.Selected
                        ElseIf (str = "OnDemand") Then
                            populateOnDemandEvent = Me.PopulateOnDemandEvent
                            If Not Object.ReferenceEquals(populateOnDemandEvent, Nothing) Then
                                populateOnDemandEvent.Invoke(Me, New DNNTreeEventArgs(objA))
                            End If
                        End If
                    Else
                        Dim flag4 As Boolean = Not Me.CheckBoxes
                        If flag4 Then
                            Dim enumerator As IEnumerator = Nothing
                            Try 
                                enumerator = Me.TreeNodes.FindSelectedNodes.GetEnumerator
                                Do While True
                                    flag4 = enumerator.MoveNext
                                    If Not flag4 Then
                                        Exit Do
                                    End If
                                    Dim current As TreeNode = DirectCast(enumerator.Current, TreeNode)
                                    current.Selected = False
                                Loop
                            Finally
                                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                                    TryCast(enumerator,IDisposable).Dispose
                                End If
                            End Try
                        End If
                        If (objA.ClickAction = eClickAction.Expand) Then
                            If (If(((objA.DNNNodes.Count <> 0) OrElse Not Me.PopulateNodesFromClient), 0, 1) <> 0) Then
                                populateOnDemandEvent = Me.PopulateOnDemandEvent
                                If Not Object.ReferenceEquals(populateOnDemandEvent, Nothing) Then
                                    populateOnDemandEvent.Invoke(Me, New DNNTreeEventArgs(objA))
                                End If
                            End If
                            If objA.IsExpanded Then
                                objA.Collapse
                            Else
                                objA.Expand
                            End If
                        End If
                        objA.Click
                    End If
                End If
            End If
        End Sub

        Public Sub RaisePostDataChangedEvent() Implements IPostBackDataHandler.RaisePostDataChangedEvent
        End Sub

        Public Sub RegisterClientScript()
            If Not Me.IsDownLevel Then
                ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_dom)
                If Me.PopulateNodesFromClient Then
                    ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_xmlhttp)
                End If
                WebControls.RegisterSubmitComponent(Me.Page)
                WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.js", FileOrder.Js.DnnControlsLabelEdit)
                WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.dnntree.js")
            End If
        End Sub

        <Obsolete("RegisterDNNVariableControl on control level is now obsolete.  Use RegisterDNNVariableControl.WebControls")> _
        Public Sub RegisterDNNVariableControl()
            ClientAPI.RegisterDNNVariableControl(Me)
        End Sub

        Protected Overrides Sub Render(ByVal writer As HtmlTextWriter)
            Dim enumerator As IEnumerator = Nothing
            Me.TreeWriter.RenderTree(writer, Me)
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
                Dim properties As Hashtable = Me.TreeWriter.MarshalledProperties
                Dim marshalledPropertyJSON As String = ScriptGenerator.GetMarshalledPropertyJSON(Me, properties)
                BaseWebControl.RegisterInitialize(Me, "initTree", marshalledPropertyJSON)
            End If
        End Sub

        Protected Overrides Function SaveViewState() As Object
            Return New Object() { MyBase.SaveViewState, DirectCast(Me.ImageList, IStateManager).SaveViewState }
        End Function

        Public Function SelectNode(ByVal strID As String) As TreeNode
            Dim objA As TreeNode = Nothing
            If (Me.TreeNodes.Count > 0) Then
                objA = Me.FindNode(strID)
                If Not Object.ReferenceEquals(objA, Nothing) Then
                    objA.Selected = True
                    Dim parent As TreeNode = objA.Parent
                    Do While True
                        If Object.ReferenceEquals(parent, Nothing) Then
                            Exit Do
                        End If
                        parent.Expand
                        parent = parent.Parent
                    Loop
                End If
            End If
            Return objA
        End Function

        Public Function SelectNodeByKey(ByVal strKey As String) As TreeNode
            Dim objA As TreeNode = Nothing
            If (Me.TreeNodes.Count > 0) Then
                objA = Me.FindNodeByKey(strKey)
                If Not Object.ReferenceEquals(objA, Nothing) Then
                    objA.Selected = True
                    Dim parent As TreeNode = objA
                    Do While True
                        If Object.ReferenceEquals(parent, Nothing) Then
                            Exit Do
                        End If
                        parent.Expand
                        parent = parent.Parent
                    Loop
                End If
            End If
            Return objA
        End Function

        Protected Overrides Sub TrackViewState()
            MyBase.TrackViewState
        End Sub

        Private Sub UpdateNodes(ByVal objNodes As TreeNodeCollection)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = objNodes.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As TreeNode = DirectCast(enumerator.Current, TreeNode)
                    If (Strings.Len(current.Image) > 0) Then
                        current.ImageIndex = If(Me.ImageList.Contains(current.Image), Me.ImageList.IndexOf(current.Image), Me.ImageList.Add(current.Image))
                        current.Image = Nothing
                    End If
                    If (current.DNNNodes.Count > 0) Then
                        Me.UpdateNodes(current.TreeNodes)
                    End If
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
        End Sub


        ' Properties
        <ClientProperty, ClientPropertyName("bid"), DefaultValue(""), Bindable(True)> _
        Public Property BehaviorID As String
            Get
                Return Conversions.ToString(Me.ViewState("BehaviorID"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("BehaviorID") = Value
            End Set
        End Property

        <DefaultValue(0), ClientProperty, ClientPropertyName("cbm")> _
        Public Property CheckBoxMode As eCheckBoxMode
            Get
                Return If((Strings.Len(Me.ViewState("CheckBoxMode")) <= 0), eCheckBoxMode.MultiSelect, DirectCast(Conversions.ToInteger(Me.ViewState("CheckBoxMode")), eCheckBoxMode))
            End Get
            Set(ByVal Value As eCheckBoxMode)
                Me.ViewState("CheckBoxMode") = Value
            End Set
        End Property

        Public Property ForceDownLevel As Boolean
            Get
                Return Conversions.ToBoolean(Me.ViewState("ForceDownLevel"))
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("ForceDownLevel") = Value
            End Set
        End Property

        Public Property IsCrawler As Boolean
            Get
                Return If((Strings.Len(Me.ViewState("IsCrawler")) <> 0), Conversions.ToBoolean(Me.ViewState("IsCrawler")), HttpContext.Current.Request.Browser.Crawler)
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("IsCrawler") = Value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property IsDownLevel As Boolean
            Get
                Return (Me.ForceDownLevel OrElse (Me.IsCrawler OrElse (Not ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML) OrElse Not ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.XML))))
            End Get
        End Property

        Public Property ClientAPIScriptPath As String
            Get
                Return ClientAPI.ScriptPath
            End Get
            Set(ByVal Value As String)
                ClientAPI.ScriptPath = Value
            End Set
        End Property

        Public Property TreeScriptPath As String
            Get
                Return If((Strings.Len(Me.ViewState("TreeScriptPath")) <> 0), Conversions.ToString(Me.ViewState("TreeScriptPath")), Me.ClientAPIScriptPath)
            End Get
            Set(ByVal Value As String)
                Me.ViewState("TreeScriptPath") = Value
            End Set
        End Property

        <UrlProperty, ClientProperty, Description("Directory to find the images for the menu.  Need to have spacer.gif here!"), DefaultValue("images/"), ClientPropertyName("sysimgpath")> _
        Public Property SystemImagesPath As String
            Get
                Return If((Strings.Len(Me.ViewState("SystemImagesPath")) <> 0), Conversions.ToString(Me.ViewState("SystemImagesPath")), "images/")
            End Get
            Set(ByVal Value As String)
                Me.ViewState("SystemImagesPath") = Value
            End Set
        End Property

        <PersistenceMode(PersistenceMode.InnerProperty), Browsable(True)> _
        Public ReadOnly Property TreeNodes As TreeNodeCollection
            Get
                If Object.ReferenceEquals(Me.m_Nodes, Nothing) Then
                    Me.m_Nodes = New TreeNodeCollection(Me.ClientID, Me)
                End If
                Return Me.m_Nodes
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property SelectedTreeNodes As Collection
            Get
                Return Me.TreeNodes.FindSelectedNodes
            End Get
        End Property

        <Bindable(True), ClientPropertyName("indentw"), ClientProperty, PersistenceMode(PersistenceMode.Attribute), DefaultValue(10)> _
        Public Property IndentWidth As Integer
            Get
                Return Conversions.ToInteger(Me.ViewState("IndentWidth"))
            End Get
            Set(ByVal Value As Integer)
                Me.ViewState("IndentWidth") = Value
            End Set
        End Property

        <Bindable(True), PersistenceMode(PersistenceMode.Attribute), DefaultValue(""), ClientPropertyName("colimg"), UrlProperty, ClientProperty> _
        Public Property CollapsedNodeImage As String
            Get
                Return Conversions.ToString(Me.ViewState("CollapsedNodeImage"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("CollapsedNodeImage") = Value
            End Set
        End Property

        <ClientPropertyName("expimg"), UrlProperty, ClientProperty, PersistenceMode(PersistenceMode.Attribute), DefaultValue(""), Bindable(True)> _
        Public Property ExpandedNodeImage As String
            Get
                Return Conversions.ToString(Me.ViewState("ExpandedNodeImage"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("ExpandedNodeImage") = Value
            End Set
        End Property

        <DefaultValue("dnnanim.gif"), Bindable(True), PersistenceMode(PersistenceMode.Attribute), ClientPropertyName("workimg"), ClientProperty> _
        Public Property WorkImage As String
            Get
                Return Conversions.ToString(Me.ViewState("WorkImage"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("WorkImage") = Value
            End Set
        End Property

        <ClientProperty, Category("Behavior"), PersistenceMode(PersistenceMode.Attribute), DefaultValue(5), Bindable(True), ClientPropertyName("animf")> _
        Public Property AnimationFrames As Integer
            Get
                Return If((Strings.Len(Me.ViewState("AnimationFrames")) <= 0), 5, Conversions.ToInteger(Me.ViewState("AnimationFrames")))
            End Get
            Set(ByVal Value As Integer)
                Me.ViewState("AnimationFrames") = Value
            End Set
        End Property

        <DefaultValue(False), Bindable(True), ClientProperty, PersistenceMode(PersistenceMode.Attribute), ClientPropertyName("checkboxes")> _
        Public Property CheckBoxes As Boolean
            Get
                Return Conversions.ToBoolean(Me.ViewState("CheckBoxes"))
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("CheckBoxes") = Value
            End Set
        End Property

        <ClientPropertyName("target"), DefaultValue(""), ClientProperty, Bindable(True)> _
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
                If Object.ReferenceEquals(Me.m_objImages, Nothing) Then
                    Me.m_objImages = New NodeImageCollection
                    If Me.IsTrackingViewState Then
                        DirectCast(Me.m_objImages, IStateManager).TrackViewState
                    End If
                End If
                Return Me.m_objImages
            End Get
        End Property

        <PersistenceMode(PersistenceMode.Attribute), ClientPropertyName("css"), DefaultValue(""), Bindable(True), ClientProperty> _
        Public Property DefaultNodeCssClass As String
            Get
                Return Me.CssClass
            End Get
            Set(ByVal Value As String)
                Me.CssClass = Value
            End Set
        End Property

        <Bindable(True), DefaultValue(""), ClientPropertyName("csschild"), ClientProperty, PersistenceMode(PersistenceMode.Attribute)> _
        Public Property DefaultChildNodeCssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("DefaultChildNodeCssClass"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("DefaultChildNodeCssClass") = Value
            End Set
        End Property

        <DefaultValue(""), Bindable(True), ClientPropertyName("csshover"), ClientProperty, PersistenceMode(PersistenceMode.Attribute)> _
        Public Property DefaultNodeCssClassOver As String
            Get
                Return Conversions.ToString(Me.ViewState("DefaultNodeCssClassOver"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("DefaultNodeCssClassOver") = Value
            End Set
        End Property

        <ClientProperty, ClientPropertyName("csssel"), PersistenceMode(PersistenceMode.Attribute), DefaultValue(""), Bindable(True)> _
        Public Property DefaultNodeCssClassSelected As String
            Get
                Return Conversions.ToString(Me.ViewState("DefaultNodeCssClassSelected"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("DefaultNodeCssClassSelected") = Value
            End Set
        End Property

        <DefaultValue(""), PersistenceMode(PersistenceMode.Attribute), Bindable(True), ClientPropertyName("cssicon"), ClientProperty> _
        Public Property DefaultIconCssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("DefaultIconCssClass"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("DefaultIconCssClass") = Value
            End Set
        End Property

        <Bindable(True), DefaultValue(12), ClientProperty, ClientPropertyName("expcolimgw"), PersistenceMode(PersistenceMode.Attribute)> _
        Public Property ExpandCollapseImageWidth As Integer
            Get
                Return If((Strings.Len(Me.ViewState("ExpColImgWidth")) <= 0), 12, Conversions.ToInteger(Me.ViewState("ExpColImgWidth")))
            End Get
            Set(ByVal Value As Integer)
                Me.ViewState("ExpColImgWidth") = Value
            End Set
        End Property

        Private ReadOnly Property TreeWriter As IDNNTreeWriter
            Get
                If Object.ReferenceEquals(Me.m_TreeWriter, Nothing) Then
                    Me.m_TreeWriter = If(Not Me.IsDownLevel, DirectCast(New DNNTreeUpLevelWriter, IDNNTreeWriter), DirectCast(New DNNTreeWriter, IDNNTreeWriter))
                End If
                Return Me.m_TreeWriter
            End Get
        End Property

        <DefaultValue(""), ClientPropertyName("js"), ClientProperty> _
        Public Property JSFunction As String
            Get
                Return Conversions.ToString(Me.ViewState("JSFunction"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("JSFunction") = Value
            End Set
        End Property

        <PersistenceMode(PersistenceMode.Attribute), DefaultValue(True), Bindable(False)> _
        Public Property PopulateNodesFromClient As Boolean
            Get
                Return If((Not Me.ViewState("PopNodesFromClient") Is Nothing), Conversions.ToBoolean(Me.ViewState("PopNodesFromClient")), True)
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


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private m_Nodes As TreeNodeCollection
        Private m_objImages As NodeImageCollection
        Private m_TreeWriter As IDNNTreeWriter

        ' Nested Types
        Public Delegate Sub DNNTreeEventHandler(ByVal source As Object, ByVal e As DNNTreeEventArgs)

        Public Delegate Sub DNNTreeNodeClickHandler(ByVal source As Object, ByVal e As DNNTreeNodeClickEventArgs)
    End Class
End Namespace

