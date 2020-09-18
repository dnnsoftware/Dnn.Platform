' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System.Xml.Serialization

Namespace DotNetNuke.UI.Utilities

    <XmlRoot("browser")> _
    Public Class Browser

        Private _contains As String
        Private _name As String
        Private _minVersion As Double

        <XmlAttribute("contains")> _
        Public Property Contains() As String
            Get
                Return _contains
            End Get
            Set(ByVal Value As String)
                _contains = Value
            End Set
        End Property

        <XmlAttribute("nm")> _
        Public Property Name() As String
            Get
                Return _name
            End Get
            Set(ByVal Value As String)
                _name = Value
            End Set
        End Property

        <XmlAttribute("minversion")> _
        Public Property MinVersion() As Double
            Get
                Return _minVersion
            End Get
            Set(ByVal Value As Double)
                _minVersion = Value
            End Set
        End Property

    End Class

End Namespace

