' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Namespace DotNetNuke.UI.Utilities

    Public Interface IClientAPICallbackEventHandler
        Function RaiseClientAPICallbackEvent(ByVal eventArgument As String) As String
    End Interface

End Namespace
