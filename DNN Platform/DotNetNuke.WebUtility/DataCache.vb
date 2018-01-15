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

Imports System.Web
'Copied from DotNetNuke Core

Namespace DotNetNuke.UI.Utilities
	Public Class DataCache

		Public Shared Function GetCache(ByVal CacheKey As String) As Object

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			Return objCache(CacheKey)

		End Function

		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			objCache.Insert(CacheKey, objObject)

		End Sub

		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object, ByVal objDependency As System.Web.Caching.CacheDependency)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			objCache.Insert(CacheKey, objObject, objDependency)

		End Sub
		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object, ByVal objDependency As System.Web.Caching.CacheDependency, ByVal AbsoluteExpiration As Date, ByVal SlidingExpiration As System.TimeSpan)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			objCache.Insert(CacheKey, objObject, objDependency, AbsoluteExpiration, SlidingExpiration)

		End Sub


		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object, ByVal SlidingExpiration As Integer)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			objCache.Insert(CacheKey, objObject, Nothing, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(SlidingExpiration))

		End Sub

		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object, ByVal AbsoluteExpiration As Date)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			objCache.Insert(CacheKey, objObject, Nothing, AbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration)

		End Sub

		Public Shared Sub RemoveCache(ByVal CacheKey As String)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			If Not objCache(CacheKey) Is Nothing Then
				objCache.Remove(CacheKey)
			End If

		End Sub
	End Class
End Namespace
