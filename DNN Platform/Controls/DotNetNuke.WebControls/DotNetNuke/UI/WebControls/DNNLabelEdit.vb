' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
    Public Class DNNLabelEdit
        Inherits Label
        Implements IPostBackEventHandler, IClientAPICallbackEventHandler, IDNNToolBar, IDNNToolBarSupportedActions
        ' Events
        Public Event UpdateLabel As DNNLabelEditEventHandler

        ' Methods
        Public Sub New()
            AddHandler MyBase.PreRender, New EventHandler(AddressOf Me.DNNLabelEdit_PreRender)
            DNNLabelEdit.__ENCAddToList(Me)
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
            SyncLock DNNLabelEdit.__ENCList
                If (DNNLabelEdit.__ENCList.Count = DNNLabelEdit.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNLabelEdit.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNLabelEdit.__ENCList.RemoveRange(index, (DNNLabelEdit.__ENCList.Count - index))
                            DNNLabelEdit.__ENCList.Capacity = DNNLabelEdit.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNLabelEdit.__ENCList(num2)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNLabelEdit.__ENCList(index) = DNNLabelEdit.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNLabelEdit.__ENCList.Add(New WeakReference(value))
            End SyncLock
        End Sub

        Private Sub DNNLabelEdit_PreRender(ByVal sender As Object, ByVal e As EventArgs)
            Me.RegisterClientScript
        End Sub

        Protected Overrides Sub LoadViewState(ByVal state As Object)
            If (Not state Is Nothing) Then
                Dim objArray As Object() = DirectCast(state, Object())
                If (Not objArray(0) Is Nothing) Then
                    MyBase.LoadViewState(objArray(0))
                End If
            End If
        End Sub

        Protected Function MarshalledProperties() As Hashtable
            Dim hashtable2 As New Hashtable
            If Me.EditEnabled Then
                If ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.XMLHTTP) Then
                    hashtable2.Add("callback", ClientAPI.GetCallbackEventReference(Me, "'[TEXT]'", "this.callBackSuccess", "this", "this.callBackFail", "this.callBackStatus", Me.CallBackPostChildrenOf, Me.CallBackType))
                End If
                If (Strings.Len(Me.ToolBarId) > 0) Then
                    hashtable2.Add("tbId", Me.ToolBarId)
                    If (Strings.Len(Me.UniqueID.Replace(Me.ID, "")) > 0) Then
                        hashtable2.Add("nsPrefix", Me.NamingContainer.UniqueID)
                    End If
                End If
            End If
            Return hashtable2
        End Function

        Public Function RaiseClientAPICallbackEvent(ByVal eventArgument As String) As String Implements IClientAPICallbackEventHandler.RaiseClientAPICallbackEvent
            If (Me.UrlFormat <> UrlFormatType.None) Then
                eventArgument = WebControls.StripUrlPaths(eventArgument, Me.UrlFormat, Me.Page)
            End If
            Dim e As New DNNLabelEditEventArgs(eventArgument)
            Dim updateLabelEvent As DNNLabelEditEventHandler = Me.UpdateLabelEvent
            If Not Object.ReferenceEquals(updateLabelEvent, Nothing) Then
                updateLabelEvent.Invoke(Me, e)
            End If
            Return e.ReturnedText
        End Function

        Public Overridable Sub RaisePostBackEvent(ByVal eventArgument As String) Implements IPostBackEventHandler.RaisePostBackEvent
            Dim updateLabelEvent As DNNLabelEditEventHandler = Me.UpdateLabelEvent
            If Not Object.ReferenceEquals(updateLabelEvent, Nothing) Then
                updateLabelEvent.Invoke(Me, New DNNLabelEditEventArgs(eventArgument))
            End If
        End Sub

        Public Sub RegisterClientScript()
            If (If((Me.IsDownLevel OrElse Not Me.EditEnabled), 0, 1) <> 0) Then
                ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_dom)
                ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_dom_positioning)
                ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_xmlhttp)
                WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.js", FileOrder.Js.DnnControlsLabelEdit)
                WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.dnnlabeledit.js", FileOrder.Js.DnnModalPopup)
                If Not ClientAPI.UseExternalScripts Then
                    ClientAPI.RegisterEmbeddedResource(Me.Page, "dnn.controls.dnninputtext.js", Me.GetType)
                    ClientAPI.RegisterEmbeddedResource(Me.Page, "dnn.controls.dnnrichtext.js", Me.GetType)
                End If
            End If
        End Sub

        Protected Overrides Sub Render(ByVal writer As HtmlTextWriter)
            MyBase.Render(writer)
            If (If((Me.IsDownLevel OrElse Not Me.EditEnabled), 0, 1) <> 0) Then
                Dim properties As Hashtable = Me.MarshalledProperties
                Dim marshalledPropertyJSON As String = ScriptGenerator.GetMarshalledPropertyJSON(Me, properties)
                BaseWebControl.RegisterInitialize(Me, "initLabelEdit", marshalledPropertyJSON)
            End If
        End Sub

        Protected Overrides Function SaveViewState() As Object
            Return New Object() { MyBase.SaveViewState }
        End Function

        Protected Overrides Sub TrackViewState()
            MyBase.TrackViewState
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

        <ClientProperty, Category("Behavior"), ClientPropertyName("blursave"), DefaultValue(True)> _
        Public Property LostFocusSave As Boolean
            Get
                Return If((Strings.Len(Me.ToolBarId) <= 0), If((Strings.Len(Conversions.ToString(Me.ViewState("LostFocusSave"))) <> 0), Conversions.ToBoolean(Me.ViewState("LostFocusSave")), True), False)
            End Get
            Set(ByVal value As Boolean)
                Me.ViewState("LostFocusSave") = value
            End Set
        End Property

        <Category("Appearance"), Browsable(False)> _
        Public ReadOnly Property IsDownLevel As Boolean
            Get
                Return (Not Me.EditEnabled OrElse (Not ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML) OrElse Not ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.XML)))
            End Get
        End Property

        <PersistenceMode(PersistenceMode.Attribute), Category("Behavior"), DefaultValue(True)> _
        Public Property EditEnabled As Boolean
            Get
                Return ((Strings.Len(Me.ViewState("EditEnabled")) <= 0) OrElse Conversions.ToBoolean(Me.ViewState("EditEnabled")))
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("EditEnabled") = Value
            End Set
        End Property

        <Category("Paths")> _
        Public Property ClientAPIScriptPath As String
            Get
                Return ClientAPI.ScriptPath
            End Get
            Set(ByVal Value As String)
                ClientAPI.ScriptPath = Value
            End Set
        End Property

        <Category("Paths")> _
        Public Property LabelEditScriptPath As String
            Get
                Return If((Strings.Len(Me.ViewState("LabelEditScriptPath")) <> 0), Conversions.ToString(Me.ViewState("LabelEditScriptPath")), Me.ClientAPIScriptPath)
            End Get
            Set(ByVal Value As String)
                Me.ViewState("LabelEditScriptPath") = Value
            End Set
        End Property

        <Category("Paths"), ClientProperty, DefaultValue("images/"), Description("Directory to find the images for the LabelEdit.  Need to have spacer.gif here!"), ClientPropertyName("sysimgpath")> _
        Public Property SystemImagesPath As String
            Get
                Return If((Strings.Len(Me.ViewState("SystemImagesPath")) <> 0), Conversions.ToString(Me.ViewState("SystemImagesPath")), "images/")
            End Get
            Set(ByVal Value As String)
                Me.ViewState("SystemImagesPath") = Value
            End Set
        End Property

        <Description("Client-side event that will trigger an edit.  (onclick, ondblclick, none)"), Bindable(True), ClientPropertyName("eventName"), ClientProperty, Category("Behavior"), PersistenceMode(PersistenceMode.Attribute), DefaultValue("onclick")> _
        Public Property EventName As String
            Get
                Return If((Strings.Len(Me.ViewState("EventName")) <> 0), Conversions.ToString(Me.ViewState("EventName")), "onclick")
            End Get
            Set(ByVal Value As String)
                Me.ViewState("EventName") = Value
            End Set
        End Property

        <Category("Behavior"), DefaultValue(True), ClientPropertyName("saveonenter"), ClientProperty, PersistenceMode(PersistenceMode.Attribute)> _
        Public Property SaveOnEnter As Boolean
            Get
                Return If((Strings.Len(Me.ViewState("SaveOnEnter")) <> 0), Conversions.ToBoolean(Me.ViewState("SaveOnEnter")), True)
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("SaveOnEnter") = Value
            End Set
        End Property

        <ClientProperty, PersistenceMode(PersistenceMode.Attribute), DefaultValue(False), Category("Behavior"), ClientPropertyName("multiline")> _
        Public Property MultiLine As Boolean
            Get
                Return If((Strings.Len(Me.ViewState("MultiLine")) <> 0), Conversions.ToBoolean(Me.ViewState("MultiLine")), False)
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("MultiLine") = Value
            End Set
        End Property

        <PersistenceMode(PersistenceMode.Attribute), Category("Behavior"), DefaultValue(1)> _
        Public Property UrlFormat As UrlFormatType
            Get
                Return If((Strings.Len(Me.ViewState("UrlFormat")) <> 0), DirectCast(Conversions.ToInteger(Me.ViewState("UrlFormat")), UrlFormatType), UrlFormatType.AbsoluteWithoutServer)
            End Get
            Set(ByVal Value As UrlFormatType)
                Me.ViewState("UrlFormat") = Value
            End Set
        End Property

        <ClientPropertyName("richtext"), DefaultValue(False), ClientProperty, Category("Behavior"), PersistenceMode(PersistenceMode.Attribute)> _
        Public Property RichTextEnabled As Boolean
            Get
                Return If((Strings.Len(Me.ViewState("RichTextEnabled")) <> 0), Conversions.ToBoolean(Me.ViewState("RichTextEnabled")), False)
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("RichTextEnabled") = Value
            End Set
        End Property

        <Bindable(True), ClientPropertyName("callbackSF"), ClientProperty, Category("Behavior"), PersistenceMode(PersistenceMode.Attribute)> _
        Public Property CallbackStatusFunction As String
            Get
                Return Conversions.ToString(Me.ViewState("CallbackStatusFunction"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("CallbackStatusFunction") = Value
            End Set
        End Property

        <Category("Behavior"), Description("Image to display during a callback"), DefaultValue(0), Bindable(True)> _
        Public Property CallBackType As ClientAPICallBackResponse.CallBackTypeCode
            Get
                Return If((Strings.Len(Me.ViewState("CallBackTypeCode")) <= 0), ClientAPICallBackResponse.CallBackTypeCode.Simple, DirectCast(Conversions.ToInteger(Me.ViewState("CallBackTypeCode")), ClientAPICallBackResponse.CallBackTypeCode))
            End Get
            Set(ByVal Value As ClientAPICallBackResponse.CallBackTypeCode)
                Me.ViewState("CallBackTypeCode") = Value
            End Set
        End Property

        <Bindable(True), Description("This property allows for data on page to be posted in callback"), Category("Behavior")> _
        Public Property CallBackPostChildrenOf As String
            Get
                Return If((Strings.Len(Me.ViewState("CallBackPostChildrenOf")) <= 0), "", Conversions.ToString(Me.ViewState("CallBackPostChildrenOf")))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("CallBackPostChildrenOf") = Value
            End Set
        End Property

        <Category("Behavior")> _
        Public Property ToolBarId As String Implements IDNNToolBar.ToolBarId
            Get
                Return Conversions.ToString(Me.ViewState("ToolBarId"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("ToolBarId") = Value
                TypeDescriptor.Refresh(Me)
            End Set
        End Property

        <Browsable(False)> _
        Friend ReadOnly Property ToolBarSupportedActions As String() Implements IDNNToolBarSupportedActions.Actions
            Get
                Dim strArray2 As String() = New String() { "edit", "save", "cancel", "bold", "italic", "underline", "justifyleft", "justifycenter", "justifyright" }
                strArray2(9) = "insertorderedlist"
                strArray2(10) = "insertunorderedlist"
                strArray2(11) = "outdent"
                strArray2(12) = "indent"
                strArray2(13) = "createlink"
                Return strArray2
            End Get
        End Property

        <DefaultValue("onmousemove"), ClientPropertyName("tbEvent"), ClientProperty, Description("Allows the client-side event that displays the toolbar to be configured (onmouseover, onclick)"), Category("Behavior")> _
        Public Property ShowToolBarEventName As String
            Get
                Return If((Strings.Len(Me.ViewState("ShowToolBarEventName")) <> 0), Conversions.ToString(Me.ViewState("ShowToolBarEventName")), "onmousemove")
            End Get
            Set(ByVal Value As String)
                Me.ViewState("ShowToolBarEventName") = Value
            End Set
        End Property

        <ClientProperty, Bindable(True), PersistenceMode(PersistenceMode.Attribute), Category("Behavior"), ClientPropertyName("beforeSaveF")> _
        Public Property BeforeSaveFunction As String
            Get
                Return Conversions.ToString(Me.ViewState("BeforeSaveFunction"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("BeforeSaveFunction") = Value
            End Set
        End Property

        <DefaultValue(""), Bindable(True), ClientPropertyName("cssEdit"), PersistenceMode(PersistenceMode.Attribute), ClientProperty, Category("Appearance")> _
        Public Property LabelEditCssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("LabelEditCssClass"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("LabelEditCssClass") = Value
            End Set
        End Property

        <Category("Appearance"), DefaultValue(""), Bindable(True), PersistenceMode(PersistenceMode.Attribute), ClientPropertyName("cssWork"), ClientProperty> _
        Public Property WorkCssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("WorkCssClass"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("WorkCssClass") = Value
            End Set
        End Property

        <ClientProperty, PersistenceMode(PersistenceMode.Attribute), ClientPropertyName("cssOver"), DefaultValue(""), Category("Appearance")> _
        Public Property MouseOverCssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("MouseOverCssClass"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("MouseOverCssClass") = Value
            End Set
        End Property

        <DefaultValue(False), Category("Misc")> _
        Public Property RenderAsDiv As Boolean
            Get
                Return Conversions.ToBoolean(Me.ViewState("RenderAsDiv"))
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("RenderAsDiv") = Value
            End Set
        End Property

        Protected Overrides ReadOnly Property TagName As String
            Get
                Return If(Not Me.RenderAsDiv, "span", "div")
            End Get
        End Property

        Protected Overrides ReadOnly Property TagKey As HtmlTextWriterTag
            Get
                Return If(Not Me.RenderAsDiv, HtmlTextWriterTag.Span, HtmlTextWriterTag.Div)
            End Get
        End Property


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)

        ' Nested Types
        Public Delegate Sub DNNLabelEditEventHandler(ByVal source As Object, ByVal e As DNNLabelEditEventArgs)
    End Class
End Namespace

