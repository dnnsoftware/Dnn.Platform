' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Text

Namespace DotNetNuke.UI.WebControls
    Public Class HTMLElement
        ' Methods
        Public Sub New(ByVal TagName As String)
            Me.m_strTagName = TagName
        End Sub

        Private Function SafeJSONString(ByVal strString As String) As String
            Return ("'" & Strings.Replace(Strings.Replace(Strings.Replace(strString, ChrW(13), "\r", 1, -1, CompareMethod.Binary), ChrW(10), "\n", 1, -1, CompareMethod.Binary), "'", "\'", 1, -1, CompareMethod.Binary) & "'")
        End Function

        Public Function ToJSON() As String
            Dim enumerator As IEnumerator = Nothing
            Dim builder As New StringBuilder
            Try 
                enumerator = Me.Attributes.Keys.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim str2 As String = Conversions.ToString(enumerator.Current)
                    If (builder.Length = 0) Then
                        builder.Append("{"c)
                    Else
                        builder.Append(","c)
                    End If
                    builder.Append($"{str2}:{Me.SafeJSONString(Conversions.ToString(Me.Attributes(str2)))}")
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            If (Strings.Len(Me.Text) > 0) Then
                If (builder.Length = 0) Then
                    builder.Append("{"c)
                Else
                    builder.Append(","c)
                End If
                builder.Append($"{"__text"}:{Me.SafeJSONString(Me.Text)}")
            End If
            builder.Append("}"c)
            Return builder.ToString
        End Function


        ' Properties
        Public Property Raw As String
            Get
                Return Me.m_strRaw
            End Get
            Set(ByVal Value As String)
                Me.m_strRaw = Value
            End Set
        End Property

        Public Property [Text] As String
            Get
                Return Me.m_strText
            End Get
            Set(ByVal Value As String)
                Me.m_strText = Value
            End Set
        End Property

        Public Property TagName As String
            Get
                Return Me.m_strTagName
            End Get
            Set(ByVal Value As String)
                Me.m_strTagName = Value
            End Set
        End Property

        Public ReadOnly Property Attributes As Hashtable
            Get
                Return Me.m_objAttributes
            End Get
        End Property


        ' Fields
        Private m_strRaw As String
        Private m_strText As String
        Private m_strTagName As String
        Private m_objAttributes As Hashtable = New Hashtable
    End Class
End Namespace

