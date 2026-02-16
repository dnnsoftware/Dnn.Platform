' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.ComponentModel
Imports System.Drawing
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    Public Class ScriptGenerator
        ' Methods
        Public Shared Function GetMarshalledProperties(ByVal instance As Object) As Hashtable
            Return ScriptGenerator.GetMarshalledProperties(instance, New Hashtable)
        End Function

        Public Shared Function GetMarshalledProperties(ByVal instance As Object, ByVal properties As Hashtable) As Hashtable
            Dim objA As ClientEventAttribute = Nothing
            Dim attribute2 As ClientPropertyNameAttribute = Nothing
            Dim attribute3 As ClientPropertyAttribute = Nothing
            Dim enumerator As IEnumerator = Nothing
            Dim service As IUrlResolutionService = DirectCast(instance, IUrlResolutionService)
            Dim key As String = ""
            Dim descriptors As PropertyDescriptorCollection = TypeDescriptor.GetProperties(instance)
            Try 
                enumerator = descriptors.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As PropertyDescriptor = DirectCast(enumerator.Current, PropertyDescriptor)
                    attribute3 = TryCast(current.Attributes(GetType(ClientPropertyAttribute)),ClientPropertyAttribute)
                    Dim flag As Boolean = True
                    If Object.ReferenceEquals(attribute3, Nothing) Then
                        objA = TryCast(current.Attributes(GetType(ClientEventAttribute)),ClientEventAttribute)
                        If Object.ReferenceEquals(objA, Nothing) Then
                            flag = False
                        End If
                    End If
                    If flag Then
                        key = current.Name
                        attribute2 = TryCast(current.Attributes(GetType(ClientPropertyNameAttribute)),ClientPropertyNameAttribute)
                        If Not Object.ReferenceEquals(attribute2, Nothing) Then
                            key = attribute2.Name
                        End If
                        If (current.ShouldSerializeValue(instance) OrElse current.IsReadOnly) Then
                            Dim obj2 As Object = current.GetValue(instance)
                            If (If(((objA Is Nothing) OrElse Object.ReferenceEquals(current.PropertyType, GetType(String))), 0, 1) <> 0) Then
                                Throw New InvalidOperationException("Events properties can only be of type string")
                            End If
                            If ((If((current.PropertyType.IsPrimitive OrElse current.PropertyType.IsEnum), 0, 1) <> 0) AndAlso Object.ReferenceEquals(current.PropertyType, GetType(Color))) Then
                                Dim color1 As Color
                                If (Not obj2 Is Nothing) Then
                                    color1 = DirectCast(obj2, Color)
                                Else
                                    Dim color As Color
                                    Dim local1 As Object = obj2
                                    color1 = color
                                End If
                                obj2 = ColorTranslator.ToHtml(color1)
                            End If
                            If Object.ReferenceEquals(current.PropertyType, GetType(Boolean)) Then
                                obj2 = If(Not Conversions.ToBoolean(obj2), "0", "1")
                            End If
                            If (If(((current.Attributes(GetType(UrlPropertyAttribute)) Is Nothing) OrElse ((service Is Nothing) OrElse (obj2 Is Nothing))), 0, 1) <> 0) Then
                                obj2 = If(Not TypeOf instance Is Control, service.ResolveClientUrl(Conversions.ToString(obj2)), DirectCast(instance, Control).ResolveUrl(Conversions.ToString(obj2)))
                            End If
                            If Object.ReferenceEquals(current.PropertyType, GetType(Char)) Then
                                If (Strings.Asc(Conversions.ToChar(obj2)) <> 0) Then
                                    properties.Add(key, obj2)
                                End If
                            ElseIf (Not obj2 Is Nothing) Then
                                If Not Object.ReferenceEquals(objA, Nothing) Then
                                    properties.Add(key, Conversions.ToString(obj2))
                                Else
                                    properties.Add(key, obj2)
                                End If
                            End If
                        End If
                    End If
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            Return properties
        End Function

        Public Shared Function GetMarshalledPropertyJSON(ByVal instance As Object) As String
            Dim properties As New Hashtable
            Return ScriptGenerator.GetMarshalledPropertyJSON(instance, properties)
        End Function

        Public Shared Function GetMarshalledPropertyJSON(ByVal instance As Object, ByVal properties As Hashtable) As String
            Return WebControls.ToJSON(ScriptGenerator.GetMarshalledProperties(instance, properties))
        End Function

    End Class
End Namespace

