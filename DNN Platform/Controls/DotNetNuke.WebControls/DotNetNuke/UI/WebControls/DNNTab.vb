' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Utilities
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.Web.UI
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    <ParseChildren(False), TypeConverter(GetType(ExpandableObjectConverter)), PersistChildren(True)> _
    Public Class DNNTab
        Inherits WebControl
        Implements INamingContainer
        ' Events
        Public Event PreLoadPostData As PreLoadPostDataEventHandler
        Public Event SetupDefaults As SetupDefaultsEventHandler

        ' Methods
        Public Sub New()
            MyBase.New(HtmlTextWriterTag.Div)
            DNNTab.__ENCAddToList(Me)
            Me.m_oDNNTabLabel = New DNNTabLabel
        End Sub

        Public Sub New(ByVal LabelText As String)
            Me.New()
            Me.Label.Text = LabelText
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
            SyncLock DNNTab.__ENCList
                If (DNNTab.__ENCList.Count = DNNTab.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNTab.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNTab.__ENCList.RemoveRange(index, (DNNTab.__ENCList.Count - index))
                            DNNTab.__ENCList.Capacity = DNNTab.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNTab.__ENCList(num2)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNTab.__ENCList(index) = DNNTab.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNTab.__ENCList.Add(New WeakReference(value))
            End SyncLock
        End Sub

        Protected Overrides Sub AddAttributesToRender(ByVal writer As HtmlTextWriter)
            If Not Me.IsSelected Then
                Me.Style.Add("display", "none")
            End If
            MyBase.AddAttributesToRender(writer)
            If (If(((Strings.Len(Me.ParentControl.DefaultContainerCssClass) <= 0) OrElse (Strings.Len(Me.CssClass) <> 0)), 0, 1) <> 0) Then
                writer.AddAttribute("class", Me.ParentControl.DefaultContainerCssClass)
            End If
        End Sub

        Protected Overrides Sub LoadViewState(ByVal savedState As Object)
            If (savedState Is Nothing) Then
                MyBase.LoadViewState(savedState)
            Else
                Dim objArray As Object() = DirectCast(savedState, Object())
                If (objArray.Length <> 2) Then
                    Throw New ArgumentException("Invalid View State")
                End If
                If (Not objArray(0) Is Nothing) Then
                    MyBase.LoadViewState(objArray(0))
                End If
                If (Not objArray(1) Is Nothing) Then
                    DirectCast(Me.Label, IStateManager).LoadViewState(objArray(1))
                End If
            End If
        End Sub

        Friend Function MarshalledProperties() As Hashtable
            Dim hashtable2 As New Hashtable From { { "tid", Me.ID }, { "tcid", Me.ClientID } }
            If (Not WebControls.IsDesignMode AndAlso Not Me.ParentControl.IsDownLevel) Then
                If (Me.TabRenderMode = DNNTabStrip.eTabRenderMode.CallBack) Then
                    Select Case Me.TabCallbackPostMode
                        Case eTabCallbackPostMode.DNNVariable
                            hashtable2.Add("postmode", "__dnnVariable")
                            Exit Select
                        Case eTabCallbackPostMode.TabStrip
                            hashtable2.Add("postmode", Me.ParentControl.ClientID)
                            Exit Select
                        Case eTabCallbackPostMode.Form
                            Dim parentControl As Control = Me.ParentControl
                            Do While True
                                If Not TypeOf parentControl Is HtmlForm Then
                                    parentControl = parentControl.Parent
                                    If Not Object.ReferenceEquals(parentControl, Nothing) Then
                                        Continue Do
                                    End If
                                End If
                                If Object.ReferenceEquals(parentControl, Nothing) Then
                                    Throw New InvalidOperationException("Could not find form control")
                                End If
                                hashtable2.Add("postmode", parentControl.ClientID)
                                Exit Do
                            Loop
                            Exit Select
                        Case Else
                            Exit Select
                    End Select
                End If
                If (Me.CallBackType <> Me.ParentControl.CallBackType) Then
                    hashtable2.Add("cbtype", CInt(Me.CallBackType).ToString(CultureInfo.InvariantCulture))
                End If
            End If
            Return hashtable2
        End Function

        Friend Sub RaisePreLoadPostData()
            Dim preLoadPostDataEvent As PreLoadPostDataEventHandler = Me.PreLoadPostDataEvent
            If Not Object.ReferenceEquals(preLoadPostDataEvent, Nothing) Then
                preLoadPostDataEvent.Invoke
            End If
        End Sub

        Friend Sub RaiseSetupDefaultsEvent(ByVal IsCallBack As Boolean)
            Me.m_blnIsCallBack = IsCallBack
            Dim setupDefaultsEvent As SetupDefaultsEventHandler = Me.SetupDefaultsEvent
            If Not Object.ReferenceEquals(setupDefaultsEvent, Nothing) Then
                setupDefaultsEvent.Invoke
            End If
        End Sub

        Friend Sub RenderLabel(ByVal writer As HtmlTextWriter)
            Dim cssClassSelected As String = Nothing
            Dim flag As Boolean = Not Object.ReferenceEquals(Me.ParentControl, Nothing)
            Dim label As New Label
            label.Controls.Clear()
            label.ID = (Me.ClientID & "_l")
            If Not Me.IsSelected Then
                cssClassSelected = If(Me.Enabled, Me.Label.CssClass, Me.Label.CssClassDisabled)
            ElseIf (If(((Strings.Len(Me.Label.CssClassSelected) <> 0) OrElse Not flag), 0, 1) <> 0) Then
                cssClassSelected = Me.ParentControl.DefaultLabel.CssClassSelected
            End If
            If flag Then
                If Not Me.Enabled Then
                    If (Strings.Len(cssClassSelected) = 0) Then
                        cssClassSelected = Me.ParentControl.DefaultLabel.CssClassDisabled
                    End If
                Else
                    If (Strings.Len(cssClassSelected) = 0) Then
                        cssClassSelected = Me.ParentControl.CssClass
                    End If
                    If (Strings.Len(cssClassSelected) = 0) Then
                        cssClassSelected = Me.ParentControl.DefaultLabel.CssClass
                    End If
                End If
            End If
            If (Strings.Len(cssClassSelected) > 0) Then
                label.CssClass = cssClassSelected
            End If
            If Not Me.Visible Then
                label.Style.Add("display", "none")
            End If
            If Me.ParentControl.IsDownLevel Then
                label.Attributes.Add("onclick", ClientAPI.GetPostBackClientHyperlink(Me.ParentControl, (Me.ID & ClientAPI.COLUMN_DELIMITER & "OnDemand")))
            End If
            If ((Strings.Len(Me.Label.ImageUrl) > 0) OrElse (flag AndAlso (Strings.Len(Me.ParentControl.DefaultLabel.ImageUrl) > 0))) Then
                Dim child As New Image With {
                    .ID = (Me.ClientID & "_i"),
                    .ImageUrl = Me.Label.ImageUrl
                }
                If (Strings.Len(child.ImageUrl) = 0) Then
                    child.ImageUrl = Me.ParentControl.DefaultLabel.ImageUrl
                End If
                label.Controls.Add(child)
            End If
            If (If((Not flag OrElse (Strings.Len(Me.ParentControl.WorkImage) <= 0)), 0, 1) <> 0) Then
                Dim child As New Image With {
                    .ID = (Me.ClientID & "_w"),
                    .ImageUrl = Me.ParentControl.WorkImage
                }
                child.Style.Add("display", "none")
                label.Controls.Add(child)
            End If
            label.Controls.Add(New LiteralControl(Me.Label.Text))
            label.RenderControl(writer)
        End Sub

        Protected Overrides Function SaveViewState() As Object
            Dim objArray As Object() = New Object(1) {}
            objArray(0) = MyBase.SaveViewState
            objArray(1) = If(Object.ReferenceEquals(Me.m_oDNNTabLabel, Nothing), Nothing, DirectCast(Me.Label, IStateManager).SaveViewState)
            Dim num2 As Integer = (objArray.Length - 1)
            Dim index As Integer = 0
            Do While True
                Dim obj2 As Object
                Dim num3 As Integer = num2
                If (index > num3) Then
                    obj2 = Nothing
                Else
                    If (objArray(index) Is Nothing) Then
                        index += 1
                        Continue Do
                    End If
                    obj2 = objArray
                End If
                Return obj2
            Loop

            Return Nothing
        End Function

        Friend Sub SetParent(ByVal Parent As DNNTabStrip)
            Me.m_oParentTabStrip = Parent
        End Sub

        Friend Sub SetTabState(ByVal IsPostBack As Boolean)
            Me.m_blnIsPostBack = IsPostBack
        End Sub

        Protected Overrides Sub TrackViewState()
            MyBase.TrackViewState()

            If Not Object.ReferenceEquals(Me.Label, Nothing) Then
                DirectCast(Me.Label, IStateManager).TrackViewState()
            End If
        End Sub


        ' Properties
        <ClientPropertyName("enabled"), ClientProperty, DefaultValue(True)>
        Public Overrides Property Enabled As Boolean
            Get
                Return MyBase.Enabled
            End Get
            Set(ByVal value As Boolean)
                MyBase.Enabled = value
            End Set
        End Property

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public ReadOnly Property IsPostBack As Boolean
            Get
                Return Me.m_blnIsPostBack
            End Get
        End Property

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public ReadOnly Property IsCallBack As Boolean
            Get
                Return Me.m_blnIsCallBack
            End Get
        End Property

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Protected ReadOnly Property ParentControl As DNNTabStrip
            Get
                If (If(((Not Me.m_oParentTabStrip Is Nothing) OrElse Not TypeOf Me.Parent Is DNNTabStrip), 0, 1) <> 0) Then
                    Me.m_oParentTabStrip = DirectCast(Me.Parent, DNNTabStrip)
                End If
                Return Me.m_oParentTabStrip
            End Get
        End Property

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Friend ReadOnly Property IsSelected As Boolean
            Get
                Dim flag As Boolean
                If Object.ReferenceEquals(Me.ParentControl, Nothing) Then
                    flag = True
                ElseIf (If(((Me.ParentControl.SelectedTab Is Nothing) OrElse ((Me.ID <> Me.ParentControl.SelectedTab.ID) OrElse (Me.ID Is Nothing))), 0, 1) <> 0) Then
                    flag = True
                Else
                    flag = False
                End If
                Return flag
            End Get
        End Property

        <NotifyParentProperty(True), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Description("The style to be applied to Label"), Category("Style")>
        Public ReadOnly Property Label As DNNTabLabel
            Get
                Return Me.m_oDNNTabLabel
            End Get
        End Property

        <Category("Behavior"), DefaultValue(2)>
        Public Property TabRenderMode As DNNTabStrip.eTabRenderMode
            Get
                Return If((Not Me.ViewState("TabRenderMode") Is Nothing), DirectCast(Conversions.ToInteger(Me.ViewState("TabRenderMode")), DNNTabStrip.eTabRenderMode), If(Not Object.ReferenceEquals(Me.ParentControl, Nothing), Me.ParentControl.TabRenderMode, DNNTabStrip.eTabRenderMode.CallBack))
            End Get
            Set(ByVal Value As DNNTabStrip.eTabRenderMode)
                Me.ViewState("TabRenderMode") = Value
            End Set
        End Property

        <DefaultValue(0), Category("Behavior")>
        Public Property TabCallbackPostMode As eTabCallbackPostMode
            Get
                Return If((Not Me.ViewState("TabCallbackPostMode") Is Nothing), DirectCast(Conversions.ToInteger(Me.ViewState("TabCallbackPostMode")), eTabCallbackPostMode), eTabCallbackPostMode.None)
            End Get
            Set(ByVal Value As eTabCallbackPostMode)
                Me.ViewState("TabCallbackPostMode") = Value
            End Set
        End Property

        <DefaultValue(0), Description("Image to display during a callback"), Bindable(True)>
        Public Property CallBackType As ClientAPICallBackResponse.CallBackTypeCode
            Get
                Return If((Strings.Len(Me.ViewState("CallBackTypeCode")) <= 0), If(Object.ReferenceEquals(Me.ParentControl, Nothing), ClientAPICallBackResponse.CallBackTypeCode.Simple, Me.ParentControl.CallBackType), DirectCast(Conversions.ToInteger(Me.ViewState("CallBackTypeCode")), ClientAPICallBackResponse.CallBackTypeCode))
            End Get
            Set(ByVal Value As ClientAPICallBackResponse.CallBackTypeCode)
                Me.ViewState("CallBackTypeCode") = Value
            End Set
        End Property


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private m_oParentTabStrip As DNNTabStrip
        Private m_oDNNTabLabel As DNNTabLabel
        Private m_blnIsPostBack As Boolean
        Private m_blnIsCallBack As Boolean

        ' Nested Types
        Public Enum eTabCallbackPostMode
            ' Fields
            None = 0
            DNNVariable = 1
            TabStrip = 2
            Form = 3
        End Enum

        Public Delegate Sub PreLoadPostDataEventHandler()

        Public Delegate Sub SetupDefaultsEventHandler()
    End Class
End Namespace

