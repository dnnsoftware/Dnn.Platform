// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities
{
    using System;
    using System.Reflection;

    using NUnit.Framework;

    [Obsolete("Use Assert.Exception or ExpectedExceptionAttribute. Scheduled removal in v11.0.0.")]
    public static class ExceptionAssert
    {
        // public static void Throws<TException>(Action act) where TException : Exception
        // {
        //    Throws<TException>(act, ex => true);
        // }

        // public static void Throws<TException>(string message, Action act) where TException : Exception
        // {
        //    Throws<TException>(act, ex => ex.Message.Equals(message, StringComparison.Ordinal));
        // }
        public static void Throws<TException>(string message, Action act, Predicate<TException> checker)
            where TException : Exception
        {
            Throws<TException>(act, ex => ex.Message.Equals(message, StringComparison.Ordinal) && checker(ex));
        }

        public static void Throws<TException>(Action act, Predicate<TException> checker)
            where TException : Exception
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
                    if (typeof(TException) == typeof(TargetInvocationException))
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
                throw new AssertionException(string.Format("Expected exception of type '{0}' was not thrown", typeof(TException).FullName));
            }
            else if (!matched)
            {
                throw new AssertionException(string.Format("Expected exception of type '{0}' was thrown but did not match the configured criteria", typeof(TException).FullName));
            }
        }

        public static void ThrowsArgNull(string paramName, Action act)
        {
            Throws<ArgumentNullException>(act, ex => string.Equals(ex.ParamName, paramName, StringComparison.Ordinal));
        }

        public static void ThrowsArgNullOrEmpty(string paramName, Action act)
        {
            Throws<ArgumentException>(
                string.Format("Argument cannot be null or an empty string{1}Parameter name: {0}", paramName, Environment.NewLine),
                act,
                ex => string.Equals(ex.ParamName, paramName, StringComparison.Ordinal));
        }
    }
}
