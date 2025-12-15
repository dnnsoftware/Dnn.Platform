' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System.Xml.Serialization

Namespace DotNetNuke.UI.Utilities

    Public Class FunctionalityInfo

        Private _desc As String
        Private _functionality As ClientAPI.ClientFunctionality
        Private _excludes As BrowserCollection
        Private _supports As BrowserCollection

        <XmlAttribute("nm")> _
        Public Property Functionality() As ClientAPI.ClientFunctionality
            Get
                Return _functionality
            End Get
            Set(ByVal Value As ClientAPI.ClientFunctionality)
                _functionality = Value
            End Set
        End Property

        <XmlAttribute("desc")> _
        Public Property Desc() As String
            Get
                Return _desc
            End Get
            Set(ByVal Value As String)
                _desc = Value
            End Set
        End Property

        <XmlElement("excludes")> _
        Public Property Excludes() As BrowserCollection
            Get
                Return _excludes
            End Get
            Set(ByVal Value As BrowserCollection)
                _excludes = Value
            End Set
        End Property

        <XmlElement("supports")> _
        Public Property Supports() As BrowserCollection
            Get
                Return _supports
            End Get
            Set(ByVal Value As BrowserCollection)
                _supports = Value
            End Set
        End Property

        Public Function HasMatch(ByVal strAgent As String, ByVal strBrowser As String, ByVal dblVersion As Double) As Boolean
            Dim _hasMatch As Boolean = False

            'Parse through the supported browsers to find a match
            _hasMatch = HasMatch(Supports, strAgent, strBrowser, dblVersion)

            'If has Match check the excluded browsers to find a match
            If _hasMatch Then
                _hasMatch = Not HasMatch(Excludes, strAgent, strBrowser, dblVersion)
            End If

            Return _hasMatch
        End Function

        Private Shared Function HasMatch(ByVal browsers As BrowserCollection, ByVal strAgent As String, ByVal strBrowser As String, ByVal dblVersion As Double) As Boolean
            Dim _hasMatch As Boolean = False

            'Parse through the browsers to find a match based on name/minversion
            For Each browser As Browser In browsers
                'Check by browser name and min version
                If (Not String.IsNullOrEmpty(browser.Name) AndAlso browser.Name.ToLower.Equals(strBrowser.ToLower) AndAlso browser.MinVersion <= dblVersion) Then
                    _hasMatch = True
                    Exit For
                End If

                'Check for special browser name of "*"
                If (browser.Name = "*") Then
                    _hasMatch = True
                    Exit For
                End If
            Next

            If Not _hasMatch Then
                'Parse through the browsers to find a match based on contains (more expensive so only try if NoMatch
                For Each browser As Browser In browsers
                    'Check if UserAgent contains the string (Contains)
                    If Not String.IsNullOrEmpty(browser.Contains) Then
                        If strAgent.ToLower().IndexOf(browser.Contains.ToLower()) > -1 Then
                            _hasMatch = True
                            Exit For
                        End If
                    End If
                Next
            End If

            Return _hasMatch
        End Function

    End Class

End Namespace

