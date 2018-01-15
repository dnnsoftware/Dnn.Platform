'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2018
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'


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
        Public EventName As String
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
