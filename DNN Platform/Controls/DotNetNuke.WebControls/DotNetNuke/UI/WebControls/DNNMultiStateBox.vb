' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Design.WebControls
Imports DotNetNuke.UI.Utilities
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Diagnostics
Imports System.Web.UI
Imports System.Web.UI.Design
Imports System.Web.UI.WebControls
Imports DotNetNuke.Abstractions.ClientResources

Namespace DotNetNuke.UI.WebControls
    <PersistChildren(False), ParseChildren(True, "States"), ToolboxData("<{0}:DNNMultiStateBox runat=server></{0}:DNNMultiStateBox>"), Designer(GetType(DNNMultiStateBoxDesigner))> _
    Public Class DNNMultiStateBox
        Inherits CheckBox
        ' Methods
        Public Sub New()
            AddHandler MyBase.PreRender, New EventHandler(AddressOf Me.DNNMultiStateBox_PreRender)
            DNNMultiStateBox.__ENCAddToList(Me)
            Me.m_States = Nothing
            Me.m_Enabled = True
            Me.m_ValueSet = False
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
            SyncLock DNNMultiStateBox.__ENCList
                If (DNNMultiStateBox.__ENCList.Count = DNNMultiStateBox.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNMultiStateBox.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNMultiStateBox.__ENCList.RemoveRange(index, (DNNMultiStateBox.__ENCList.Count - index))
                            DNNMultiStateBox.__ENCList.Capacity = DNNMultiStateBox.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNMultiStateBox.__ENCList(num2)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNMultiStateBox.__ENCList(index) = DNNMultiStateBox.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNMultiStateBox.__ENCList.Add(New WeakReference(value))
            End SyncLock
        End Sub

        Private Sub DNNMultiStateBox_PreRender(ByVal sender As Object, ByVal e As EventArgs)
            Me.RegisterClientScript
            If Not Object.ReferenceEquals(Me.SelectedState, Nothing) Then
                Me.Text = Me.SelectedState.Text
            End If
        End Sub

        Protected Function MarshalledProperties() As Hashtable
            Return New Hashtable From { { "states", MSAJAX.Serialize(Me.States) } }
        End Function

        Friend Sub NotifyDesigner()
            If (If((Not MyBase.DesignMode OrElse (Me.States Is Nothing)), 0, 1) <> 0) Then
                Dim designer As ControlDesigner = TryCast(TryCast(MyBase.Site.Container,IDesignerHost).GetDesigner(Me),ControlDesigner)
                Dim member As PropertyDescriptor = Nothing
                Try 
                    member = TypeDescriptor.GetProperties(Me)("States")
                Catch exception1 As Exception
                    Dim ex As Exception = exception1
                    ProjectData.SetProjectError(ex)
                    Dim exception As Exception = ex
                    ProjectData.ClearProjectError
                    Return
                End Try
                Dim ce As New ComponentChangedEventArgs(Me, member, Nothing, Me.States)
                designer.OnComponentChanged(Me, ce)
            End If
        End Sub

        Public Sub RegisterClientScript()
            ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_dom)
            WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.js", DotNetNuke.Web.Client.FileOrder.Js.DnnControlsLabelEdit)
            WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.dnnmultistatebox.js")
        End Sub

        Protected Overrides Sub Render(ByVal writer As HtmlTextWriter)
            writer.AddAttribute("value", Me.SelectedStateKey)
            MyBase.Render(writer)
            Dim properties As Hashtable = Me.MarshalledProperties
            Dim marshalledPropertyJSON As String = ScriptGenerator.GetMarshalledPropertyJSON(Me, properties)
            BaseWebControl.RegisterInitialize(Me, "initMultiStateBox", marshalledPropertyJSON)
        End Sub


        ' Properties
        <DefaultValue(""), ClientProperty, ClientPropertyName("state")> _
        Public Property SelectedStateKey As String
            Get
                If (If((Me.m_ValueSet OrElse (Not Me.Page.IsPostBack OrElse (Strings.Len(Me.Page.Request.Form(Me.UniqueID)) <= 0))), 0, 1) <> 0) Then
                    Me.SelectedStateKey = Me.Page.Request.Form(Me.UniqueID)
                End If
                Return If((If(((Strings.Len(Me.Attributes("value")) <= 0) OrElse (Me.Attributes("value") = "on")), 0, 1) = 0), If((Me.States.Count <= 0), "", Me.States(0).Key), Me.Attributes("value"))
            End Get
            Set(ByVal Value As String)
                Me.m_ValueSet = True
                Me.Attributes("value") = Value
            End Set
        End Property

        Public ReadOnly Property SelectedState As DNNMultiState
            Get
                Dim selectedStateKey As String = Me.SelectedStateKey
                If Not String.IsNullOrEmpty(selectedStateKey) Then
                    Dim enumerator As IEnumerator = Nothing
                    Try 
                        enumerator = Me.States.GetEnumerator
                        Do While True
                            If Not enumerator.MoveNext Then
                                Exit Do
                            End If
                            Dim current As DNNMultiState = DirectCast(enumerator.Current, DNNMultiState)
                            If (current.Key = selectedStateKey) Then
                                Return current
                            End If
                        Loop
                    Finally
                        If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                            TryCast(enumerator,IDisposable).Dispose
                        End If
                    End Try
                End If
                Return Nothing
            End Get
        End Property

        <DefaultValue(""), ClientProperty, ClientPropertyName("bid")> _
        Public Property BehaviorID As String
            Get
                Return Conversions.ToString(Me.ViewState("BehaviorID"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("BehaviorID") = Value
            End Set
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

        <Description("Location of dnn.controls.DNNMultiStateBox.js file"), Category("Paths"), DefaultValue("")> _
        Public Property MultiStateBoxScriptPath As String
            Get
                Return If((Strings.Len(Me.ViewState("MultiStateBoxScriptPath")) <> 0), Conversions.ToString(Me.ViewState("MultiStateBoxScriptPath")), Me.ClientAPIScriptPath)
            End Get
            Set(ByVal Value As String)
                Me.ViewState("MultiStateBoxScriptPath") = Value
            End Set
        End Property

        <ClientPropertyName("imgpath"), UrlProperty, ClientProperty, DefaultValue("images/"), Description("Directory to find the images for the MultiStateBox."), Category("Paths")> _
        Public Property ImagePath As String
            Get
                Return If((Strings.Len(Me.ViewState("ImagePath")) <> 0), Conversions.ToString(Me.ViewState("ImagePath")), "images/")
            End Get
            Set(ByVal Value As String)
                Me.ViewState("ImagePath") = Value
            End Set
        End Property

        <PersistenceMode(PersistenceMode.InnerProperty)> _
        Public Overridable ReadOnly Property States As DNNMultiStateCollection
            Get
                If Object.ReferenceEquals(Me.m_States, Nothing) Then
                    Me.m_States = New DNNMultiStateCollection(Me)
                End If
                Return Me.m_States
            End Get
        End Property

        <DefaultValue(True), ClientPropertyName("enabled"), ClientProperty> _
        Public Shadows Property Enabled As Boolean
            Get
                Return Me.m_Enabled
            End Get
            Set(ByVal value As Boolean)
                Me.m_Enabled = value
            End Set
        End Property


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private m_States As DNNMultiStateCollection
        Private m_Enabled As Boolean
        Private m_ValueSet As Boolean
    End Class
End Namespace

