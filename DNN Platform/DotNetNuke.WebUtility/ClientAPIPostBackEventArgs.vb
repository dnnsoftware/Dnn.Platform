' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System.Diagnostics.CodeAnalysis

Namespace DotNetNuke.UI.Utilities

    ''' -----------------------------------------------------------------------------
    ''' Project	 : DotNetNuke
    ''' Class	 : ClientAPIPostBackEventArgs
    ''' 
    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Event arguments passed to a delegate associated to a client postback event 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[Jon Henning]	9/15/2004	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Class ClientAPIPostBackEventArgs
        <SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification := "Breaking change")>
        Public EventName As String
        <SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification := "Breaking change")>
        Public EventArguments As Hashtable = New Hashtable

        Public Sub New()

        End Sub

        Public Sub New(ByVal strEventArgument As String)
            Dim aryArgs() As String = Split(strEventArgument, ClientAPI.COLUMN_DELIMITER)
            Dim i As Integer

            If (aryArgs.Length > 0) Then Me.EventName = aryArgs(0)
            For i = 1 To aryArgs.Length - 1 Step 2
                Me.EventArguments.Add(aryArgs(i), aryArgs(i + 1))
            Next

        End Sub
    End Class

End Namespace
