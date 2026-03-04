' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Design.WebControls
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Text
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    <PersistChildren(True), ParseChildren(False)> _
    Public Class DNNToolBarButton
        Inherits Control
        Implements ICustomTypeDescriptor
        ' Methods
        Public Sub New()
            DNNToolBarButton.__ENCAddToList(Me)
            Me.m_blnMarked = False
            Me.CommonActions = New String() { "js", "navigate" }
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
            SyncLock DNNToolBarButton.__ENCList
                If (DNNToolBarButton.__ENCList.Count = DNNToolBarButton.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNToolBarButton.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNToolBarButton.__ENCList.RemoveRange(index, (DNNToolBarButton.__ENCList.Count - index))
                            DNNToolBarButton.__ENCList.Capacity = DNNToolBarButton.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNToolBarButton.__ENCList(num2)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNToolBarButton.__ENCList(index) = DNNToolBarButton.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNToolBarButton.__ENCList.Add(New WeakReference(value))
            End SyncLock
        End Sub

        Public Function GetAttributes() As ComponentModel.AttributeCollection Implements ICustomTypeDescriptor.GetAttributes
            Return TypeDescriptor.GetAttributes(Me.GetType)
        End Function

        Public Function GetClassName() As String Implements ICustomTypeDescriptor.GetClassName
            Return TypeDescriptor.GetClassName(Me.GetType)
        End Function

        Public Function GetComponentName() As String Implements ICustomTypeDescriptor.GetComponentName
            Return TypeDescriptor.GetComponentName(Me.GetType)
        End Function

        Public Function GetConverter() As TypeConverter Implements ICustomTypeDescriptor.GetConverter
            Return TypeDescriptor.GetConverter(Me.GetType)
        End Function

        Public Function GetDefaultEvent() As EventDescriptor Implements ICustomTypeDescriptor.GetDefaultEvent
            Return TypeDescriptor.GetDefaultEvent(Me.GetType)
        End Function

        Public Function GetDefaultProperty() As PropertyDescriptor Implements ICustomTypeDescriptor.GetDefaultProperty
            Return TypeDescriptor.GetDefaultProperty(Me.GetType)
        End Function

        Public Function GetEditor(ByVal editorBaseType As Type) As Object Implements ICustomTypeDescriptor.GetEditor
            Return TypeDescriptor.GetEditor(Me.GetType, editorBaseType)
        End Function

        Public Function GetEvents(ByVal attributes As Attribute()) As EventDescriptorCollection Implements ICustomTypeDescriptor.GetEvents
            Return TypeDescriptor.GetEvents(Me.GetType, attributes)
        End Function

        Public Function GetEvents2() As EventDescriptorCollection Implements ICustomTypeDescriptor.GetEvents
            Return TypeDescriptor.GetEvents(Me.GetType)
        End Function

        Public Function GetProperties(ByVal attributes As Attribute()) As PropertyDescriptorCollection Implements ICustomTypeDescriptor.GetProperties
            Dim enumerator As IEnumerator = Nothing
            Dim descriptors3 As New PropertyDescriptorCollection(Nothing)
            Dim properties As PropertyDescriptorCollection = TypeDescriptor.GetProperties(Me.GetType, attributes)
            Try 
                enumerator = properties.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As PropertyDescriptor = DirectCast(enumerator.Current, PropertyDescriptor)
                    Dim name As String = current.Name
                    If (name = "NavigateUrl") Then
                        If (Me.ControlAction <> "navigate") Then
                            Continue Do
                        End If
                        current.GetValue(Me)
                        descriptors3.Add(current)
                        Continue Do
                    End If
                    If (name = "JSFunction") Then
                        If (Me.ControlAction <> "js") Then
                            Continue Do
                        End If
                        current.GetValue(Me)
                        descriptors3.Add(current)
                        Continue Do
                    End If
                    If (name <> "Key") Then
                        current.GetValue(Me)
                        descriptors3.Add(current)
                        Continue Do
                    End If
                    If (Array.IndexOf(Of String)(Me.CommonActions, Me.ControlAction) > -1) Then
                        current.GetValue(Me)
                        descriptors3.Add(current)
                    End If
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            Return descriptors3
        End Function

        Public Function GetProperties2() As PropertyDescriptorCollection Implements ICustomTypeDescriptor.GetProperties
            Return TypeDescriptor.GetProperties(Me.GetType)
        End Function

        Public Function GetPropertyOwner(ByVal pd As PropertyDescriptor) As Object Implements ICustomTypeDescriptor.GetPropertyOwner
            Return Me
        End Function

        Private Sub HandlePropertyFilters(ByVal pd As PropertyDescriptor, ByVal x As PropertyDescriptorCollection)
            pd.GetValue(Me)
            x.Add(pd)
        End Sub

        Private Sub NotifyDesigner()
            If (If(((Me.Owner Is Nothing) OrElse Not WebControls.IsDesignMode), 0, 1) <> 0) Then
                Me.Owner.NotifyDesigner
            End If
        End Sub

        Protected Overrides Sub Render(ByVal writer As HtmlTextWriter)
        End Sub

        Private Function SafeJSONString(ByVal strString As String) As String
            Return ("'" & Strings.Replace(strString, "'", "\'", 1, -1, CompareMethod.Binary) & "'")
        End Function

        Public Function ToJSON() As String
            Dim enumerator As IEnumerator = Nothing
            Dim hashtable As New Hashtable
            Dim builder As New StringBuilder
            If (Strings.Len(Me.CssClass) > 0) Then
                hashtable.Add("css", Me.SafeJSONString(Me.CssClass))
            End If
            If (Strings.Len(Me.CssClassHover) > 0) Then
                hashtable.Add("cssh", Me.SafeJSONString(Me.CssClassHover))
            End If
            If (Strings.Len(Me.ImageUrl) > 0) Then
                hashtable.Add("img", Me.SafeJSONString(Me.ImageUrl))
            End If
            If (Strings.Len(Me.Key) > 0) Then
                hashtable.Add("key", Me.SafeJSONString(Me.Key))
            End If
            If (If(((Strings.Len(Me.JSFunction) <= 0) OrElse (Me.ControlAction <> "js")), 0, 1) <> 0) Then
                hashtable.Add("js", Me.SafeJSONString(Me.JSFunction))
            End If
            If (If(((Strings.Len(Me.NavigateUrl) <= 0) OrElse (Me.ControlAction <> "navigate")), 0, 1) <> 0) Then
                hashtable.Add("url", Me.SafeJSONString(Me.NavigateUrl))
            End If
            If (Strings.Len(Me.ControlAction) > 0) Then
                hashtable.Add("ca", Me.SafeJSONString(Me.ControlAction))
            End If
            If (Strings.Len(Me.Text) > 0) Then
                hashtable.Add("txt", Me.SafeJSONString(Me.Text))
            End If
            If (Strings.Len(Me.ToolTip) > 0) Then
                hashtable.Add("alt", Me.SafeJSONString(Me.ToolTip))
            End If
            If Not Me.Visible Then
                hashtable.Add("vis", If(Me.Visible, 1, 0))
            End If
            builder.Append("{"c)
            Try 
                enumerator = hashtable.Keys.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim str2 As String = Conversions.ToString(enumerator.Current)
                    If (builder.Length > 1) Then
                        builder.Append(","c)
                    End If
                    builder.Append((str2 & ":" & Conversions.ToString(hashtable(str2))))
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            builder.Append("}"c)
            Return builder.ToString
        End Function


        ' Properties
        <Browsable(False)> _
        Private ReadOnly Property Owner As DNNToolBar
            Get
                If Object.ReferenceEquals(Me.m_objToolbar, Nothing) Then
                    Me.m_objToolbar = DirectCast(Me.Parent, DNNToolBar)
                End If
                Return Me.m_objToolbar
            End Get
        End Property

        Public Property Key As String
            Get
                Return If((Array.IndexOf(Of String)(Me.CommonActions, Me.ControlAction) <= -1), Me.ControlAction, If((Not Me.MyState("Key") Is Nothing), Conversions.ToString(Me.MyState("Key")), ""))
            End Get
            Set(ByVal Value As String)
                Me.MyState("Key") = Value
            End Set
        End Property

        <Category("Appearance"), DefaultValue("")> _
        Public Property CssClass As String
            Get
                Return If((Not Me.MyState("CssClass") Is Nothing), Conversions.ToString(Me.MyState("CssClass")), "")
            End Get
            Set(ByVal Value As String)
                Me.MyState("CssClass") = Value
            End Set
        End Property

        <Category("Appearance"), DefaultValue("")> _
        Public Property CssClassHover As String
            Get
                Return If((Not Me.MyState("CssClassHover") Is Nothing), Conversions.ToString(Me.MyState("CssClassHover")), "")
            End Get
            Set(ByVal Value As String)
                Me.MyState("CssClassHover") = Value
            End Set
        End Property

        <DefaultValue(""), TypeConverter(GetType(DNNToolBarButtonActionTypeConverter)), Category("Behavior")> _
        Public Property ControlAction As String
            Get
                Return If((Not Me.MyState("ControlAction") Is Nothing), Conversions.ToString(Me.MyState("ControlAction")), "")
            End Get
            Set(ByVal Value As String)
                Me.MyState("ControlAction") = Value
                TypeDescriptor.Refresh(Me)
            End Set
        End Property

        <Category("Appearance"), DefaultValue("")> _
        Public Property [Text] As String
            Get
                Return If((Not Me.MyState("Text") Is Nothing), Conversions.ToString(Me.MyState("Text")), "")
            End Get
            Set(ByVal Value As String)
                Me.MyState("Text") = Value
            End Set
        End Property

        <DefaultValue(""), Category("Appearance")> _
        Public Property ToolTip As String
            Get
                Return If((Not Me.MyState("ToolTip") Is Nothing), Conversions.ToString(Me.MyState("ToolTip")), "")
            End Get
            Set(ByVal Value As String)
                Me.MyState("ToolTip") = Value
            End Set
        End Property

        <Category("Appearance"), DefaultValue("")> _
        Public Property ImageUrl As String
            Get
                Return If((Not Me.MyState("ImageUrl") Is Nothing), Conversions.ToString(Me.MyState("ImageUrl")), "")
            End Get
            Set(ByVal Value As String)
                Me.MyState("ImageUrl") = Value
            End Set
        End Property

        <DefaultValue(""), Category("Behavior")> _
        Public Property NavigateUrl As String
            Get
                Return If((Not Me.MyState("NavigateUrl") Is Nothing), Conversions.ToString(Me.MyState("NavigateUrl")), "")
            End Get
            Set(ByVal Value As String)
                Me.MyState("NavigateUrl") = Value
            End Set
        End Property

        <Category("Behavior"), DefaultValue("")> _
        Public Property JSFunction As String
            Get
                Return If((Not Me.MyState("JSFunction") Is Nothing), Conversions.ToString(Me.MyState("JSFunction")), "")
            End Get
            Set(ByVal Value As String)
                Me.MyState("JSFunction") = Value
            End Set
        End Property

        Private Property MyState(ByVal strKey As String) As Object
            Get
                Return Me.ViewState(strKey)
            End Get
            Set(ByVal Value As Object)
                Me.ViewState(strKey) = Value
                Me.NotifyDesigner
            End Set
        End Property


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private m_blnMarked As Boolean
        Private m_objState As StateBag
        Private m_objToolbar As DNNToolBar
        Friend CommonActions As String()
    End Class
End Namespace

