using System;
using System.Reflection;

using NUnit.Framework;

namespace DotNetNuke.Tests.Utilities
{
    [Obsolete("Use Assert.Exception or ExpectedExceptionAttribute. Scheduled removal in v11.0.0.")]
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
