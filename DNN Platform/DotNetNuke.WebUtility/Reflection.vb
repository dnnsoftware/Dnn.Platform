' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
'Imports System.Configuration
Imports System.Web.Compilation
Imports System.Reflection

Namespace DotNetNuke.UI.Utilities

    Public Class Reflection

#Region "Public Shared Methods"

        Public Shared Function CreateObject(ByVal TypeName As String, ByVal CacheKey As String) As Object

            Return CreateObject(TypeName, CacheKey, True)

        End Function

        Public Shared Function CreateObject(ByVal TypeName As String, ByVal CacheKey As String, ByVal UseCache As Boolean) As Object

            ' dynamically create the object
            Return Activator.CreateInstance(CreateType(TypeName, CacheKey, UseCache))

        End Function

        Public Shared Function CreateObject(Of T)() As T

            ' dynamically create the object
            Return Activator.CreateInstance(Of T)()

        End Function

        Public Shared Function CreateType(ByVal TypeName As String) As Type
            Return CreateType(TypeName, "", True, False)
        End Function

        Public Shared Function CreateType(ByVal TypeName As String, ByVal IgnoreErrors As Boolean) As Type
            Return CreateType(TypeName, "", True, IgnoreErrors)
        End Function

        Public Shared Function CreateType(ByVal TypeName As String, ByVal CacheKey As String, ByVal UseCache As Boolean) As Type
            Return CreateType(TypeName, CacheKey, UseCache, False)
        End Function

        Public Shared Function CreateType(ByVal TypeName As String, ByVal CacheKey As String, ByVal UseCache As Boolean, ByVal IgnoreErrors As Boolean) As Type

            If CacheKey = "" Then
                CacheKey = TypeName
            End If

            Dim objType As Type = Nothing

            ' use the cache for performance
            If UseCache Then
                objType = CType(DataCache.GetCache(CacheKey), Type)
            End If

            ' is the type in the cache?
            If objType Is Nothing Then
                Try
                    ' use reflection to get the type of the class
                    objType = BuildManager.GetType(TypeName, True, True)

                    If UseCache Then
                        ' insert the type into the cache
                        DataCache.SetCache(CacheKey, objType)
                    End If
                Catch exc As Exception
                    ' could not load the type
                    If Not IgnoreErrors Then
                        'LogException(exc)
                        Throw
                    End If
                End Try
            End If

            Return objType
        End Function

        Public Shared Function CreateInstance(ByVal Type As Type) As Object
            If Not Type Is Nothing Then
                Return Type.InvokeMember("", System.Reflection.BindingFlags.CreateInstance, Nothing, Nothing, Nothing, Nothing)
            Else
                Return Nothing
            End If
        End Function

        Public Shared Function GetProperty(ByVal Type As Type, ByVal PropertyName As String, ByVal Target As Object) As Object
            If Not Type Is Nothing Then
                Return Type.InvokeMember(PropertyName, System.Reflection.BindingFlags.GetProperty, Nothing, Target, Nothing)
            Else
                Return Nothing
            End If
        End Function

        Public Shared Sub SetProperty(ByVal Type As Type, ByVal PropertyName As String, ByVal Target As Object, ByVal Args() As Object)
            If Not Type Is Nothing Then
                Type.InvokeMember(PropertyName, System.Reflection.BindingFlags.SetProperty, Nothing, Target, Args)
            End If
        End Sub

        Public Shared Function InvokeMethod(ByVal Type As Type, ByVal MethodName As String, ByVal Target As Object, ByVal Args() As Object) As Object
            If Not Type Is Nothing Then
                'Can't use this. in case generic method defined, params match and ambiguous match found error
                'Dim method As MethodInfo = Type.GetMethod(MethodName, Type.GetTypeArray(Args))
                Dim match As Boolean = False
                Dim method As MethodInfo = Nothing
                For Each method In Type.GetMethods()
                    If method.Name = MethodName AndAlso method.IsGenericMethod = False AndAlso method.GetParameters().Length = Args.Length Then
                        If ParamsSameType(method.GetParameters(), Args) Then
                            match = True
                            Exit For
                        End If
                    End If
                Next
                If match AndAlso Not method Is Nothing Then
                    Return method.Invoke(Target, Args)
                End If
            End If
            Return Nothing
        End Function

        Private Shared Function ParamsSameType(ByVal params() As ParameterInfo, ByVal Args() As Object) As Boolean
            Dim match As Boolean = True
            For i As Integer = 0 To Args.Length - 1
                If params(i).ParameterType.IsAssignableFrom(Args(i).GetType()) = False Then
                    match = False
                    Exit For
                End If
            Next
            Return match
        End Function

        Public Shared Function InvokeGenericMethod(Of T)(ByVal Type As Type, ByVal MethodName As String, ByVal Target As Object, ByVal Args() As Object) As T
            If Not Type Is Nothing Then
                Dim method As MethodInfo = Nothing
                For Each method In Type.GetMembers()
                    If method.Name = MethodName AndAlso method.IsGenericMethod Then Exit For
                Next
                If Not method Is Nothing Then
                    Dim genMethod As MethodInfo = method.MakeGenericMethod(GetType(T))
                    Return CType(genMethod.Invoke(Target, Args), T)
                End If
            End If
            Return Nothing
        End Function

#End Region

    End Class

End Namespace
