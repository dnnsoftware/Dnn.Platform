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
using System.Collections;
using System.Collections.Generic;

namespace DotNetNuke.Common.Utilities
{
    public static class EscapedString
    {
        private const char EscapeSequence = '\\';
        private const string DoubleEscapseSequence = @"\\";
        private const char DefaultSeperator = ',';

        /// <summary>
        /// Combine the string values of the enumerable into an escaped string
        /// </summary>
        /// <param name="enumerable">An IEnumerable of values to combine</param>
        /// <returns>An escaped string that is seperated using the specified characeter.  The escape character is '\'.  
        /// The string returned by .ToString() is used as the value of each item in the IEnumerable.</returns>
        /// <remarks>The seperator char is ','</remarks>
        public static string Combine(IEnumerable enumerable)
        {
            return Combine(enumerable, DefaultSeperator);
        }

        /// <summary>
        /// Combine the string values of the enumerable into an escaped string
        /// </summary>
        /// <param name="enumerable">An IEnumerable of values to combine</param>
        /// <param name="seperator">The character to use as a seperator</param>
        /// <returns>An escaped string that is seperated using the specified characeter.  The escape character is '\'.  
        /// The string returned by .ToString() is used as the value of each item in the IEnumerable.</returns>
        public static string Combine(IEnumerable enumerable, char seperator)
        {
            string result = "";

            foreach (var item in enumerable)
            {
                var s = item.ToString();
                s = s.Replace(EscapeSequence.ToString(), EscapeSequence.ToString() + EscapeSequence);
                s = s.Replace(seperator.ToString(), EscapeSequence.ToString() + seperator);
                result += s + seperator;
            }

            return String.IsNullOrEmpty(result) ? "" : result.Substring(0, result.Length - 1);
        }

        /// <summary>
        /// Takes an escaped string and splits it into an IEnumerable of seperate strings
        /// </summary>
        /// <param name="combinedString">The string to seperate</param>
        /// <returns>IEnumerable of all the seperated strings</returns>
        /// <remarks>The escape character is '\', the seperator char is ','</remarks>
        public static IEnumerable<string> Seperate(string combinedString)
        {
            return Seperate(combinedString, DefaultSeperator);
        }

        /// <summary>
        /// Takes an escaped string and splits it into an IEnumerable of seperate strings
        /// </summary>
        /// <param name="combinedString">The string to seperate</param>
        /// <param name="seperator">The character on which to split</param>
        /// <returns>IEnumerable of all the seperated strings</returns>
        /// <remarks>The escape character is '\'</remarks>
        public static IEnumerable<string> Seperate(string combinedString, char seperator)
        {
            var result = new List<string>();

            if(String.IsNullOrEmpty(combinedString))
            {
                return result;
            }
             
            var segments = combinedString.Split(new[] {seperator});

            for(int i = 0; i < segments.Length; i++)
            {
                var current = segments[i];

                while(current.EndsWith(EscapeSequence.ToString()))
                {
                    if(EndsInEscapeMode(current))
                    {
                        i++;
                        current = current.Substring(0, current.Length - 1) + seperator + segments[i];
                    }
                    else
                    {
                        break;
                    }
                }

                result.Add(current.Replace(DoubleEscapseSequence, EscapeSequence.ToString()));
            }
            
            return result;
        }

        private static bool EndsInEscapeMode(string s)
        {
            int escapeCount = 0;

            //count the number of escape chars on end of string
            for(int i = s.Length - 1; i > -1; i--)
            {
                if(s.Substring(i, 1) == EscapeSequence.ToString())
                {
                    escapeCount++;
                }
                else
                {
                    break;
                }
            }

            return escapeCount%2 == 1; //odd count means escape mode is active
        }
    }
}