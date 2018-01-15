#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Reflection;

using NUnit.Framework;

namespace DotNetNuke.Tests.Utilities
{
    [Obsolete("Use Assert.Exception or ExpectedExceptionAttribute")]
    public static class ExceptionAssert
    {
        //public static void Throws<TException>(Action act) where TException : Exception
        //{
        //    Throws<TException>(act, ex => true);
        //}

        //public static void Throws<TException>(string message, Action act) where TException : Exception
        //{
        //    Throws<TException>(act, ex => ex.Message.Equals(message, StringComparison.Ordinal));
        //}

        public static void Throws<TException>(string message, Action act, Predicate<TException> checker) where TException : Exception
        {
            Throws<TException>(act, ex => ex.Message.Equals(message, StringComparison.Ordinal) && checker(ex));
        }

        public static void Throws<TException>(Action act, Predicate<TException> checker) where TException : Exception
        {
            bool matched = false;
            bool thrown = false;
            try
            {
                act();
            }
            catch (Exception ex)
            {
                TException tex = ex as TException;
                if (tex == null)
                {
                    if (typeof (TException) == typeof (TargetInvocationException))
                    {
                        // The only place we do special processing is TargetInvocationException, but if that's
                        // what the user expected, we don't do anything
                        throw;
                    }

                    TargetInvocationException tiex = tex as TargetInvocationException;
                    if (tiex == null)
                    {
                        throw;
                    }
                    tex = tiex.InnerException as TException;
                    if (tex == null)
                    {
                        throw;
                    }
                }
                thrown = true;
                matched = checker(tex);
                if (!matched)
                {
                    throw;
                }
            }

            if (!thrown)
            {
                throw new AssertionException(String.Format("Expected exception of type '{0}' was not thrown", typeof (TException).FullName));
            }
            else if (!matched)
            {
                throw new AssertionException(String.Format("Expected exception of type '{0}' was thrown but did not match the configured criteria", typeof (TException).FullName));
            }
        }

        public static void ThrowsArgNull(string paramName, Action act)
        {
            Throws<ArgumentNullException>(act, ex => String.Equals(ex.ParamName, paramName, StringComparison.Ordinal));
        }

        public static void ThrowsArgNullOrEmpty(string paramName, Action act)
        {
            Throws<ArgumentException>(String.Format("Argument cannot be null or an empty string{1}Parameter name: {0}", paramName, Environment.NewLine),
                                      act,
                                      ex => String.Equals(ex.ParamName, paramName, StringComparison.Ordinal));
        }
    }
}