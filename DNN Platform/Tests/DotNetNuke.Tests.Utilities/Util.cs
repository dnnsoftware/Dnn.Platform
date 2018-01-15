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
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DotNetNuke.Tests.Utilities
{
    public static class Util
    {
        public static List<int> CreateIntegerList(int count)
        {
            var list = new List<int>();
            for (int i = 0; i < count; i++)
            {
                list.Add(i);
            }
            return list;
        }

        public static TMember GetPrivateMember<TInstance, TMember>(TInstance instance, string fieldName)
        {
            Type type = typeof(TInstance);
            TMember result = default(TMember);

            const BindingFlags privateBindings = BindingFlags.NonPublic | BindingFlags.Instance;

            // retrive private field from class
            MemberInfo member = type.GetMember(fieldName, privateBindings)[0];

            if (member.MemberType == MemberTypes.Property)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    result = (TMember)property.GetValue(instance, null);
                }
            }
            else if(member.MemberType == MemberTypes.Field)
            {
                var field = member as FieldInfo;
                if (field != null)
                {
                    result = (TMember)field.GetValue(instance);
                }
            }

            return result;
        }

        public static void SetPrivateMember<TInstance, TField>(TInstance instance, string fieldName, TField value)
        {
            Type type = typeof(TInstance);

            BindingFlags privateBindings = BindingFlags.NonPublic | BindingFlags.Instance;

            // retrive private field from class
            FieldInfo field = type.GetField(fieldName, privateBindings);

            field.SetValue(instance, value);
        }

        public static string GetFileName(string testFilesFolder, string fileName)
        {
            string fullName = String.Format("{0}\\{1}", testFilesFolder, fileName);
            if (!fullName.ToLower().EndsWith(".csv") && !fullName.ToLower().EndsWith(".sql"))
            {
                fullName += ".csv";
            }

            return fullName;
        }

        public static Stream GetFileStream(string testFilesFolder, string fileName)
        {
            var filePath = GetFileName(testFilesFolder, fileName);
            FileStream stream = null;
            if (File.Exists(filePath))
            {
                stream = new FileStream(GetFileName(testFilesFolder, fileName), FileMode.Open, FileAccess.Read);
            }
            return stream;
        }

        public static string ReadStream(string testFilesFolder, string fileName)
        {
            string text = String.Empty;
            Stream stream = GetFileStream(testFilesFolder,fileName);
            if (stream != null)
            {
                using (var reader = new StreamReader(GetFileStream(testFilesFolder, fileName)))
                {
                    text = reader.ReadToEnd();
                }
            }
            return text;
        }

        public static void ReadStream(string testFilesFolder, string fileName, Action<string, string> onReadLine)
        {
            string text = String.Empty;
            var stream = GetFileStream(testFilesFolder, fileName);
            if (stream != null)
            {
                using (var reader = new StreamReader(GetFileStream(testFilesFolder, fileName)))
                {
                    string line;
                    string header = String.Empty;
                    int count = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        //Ignore first line
                        if (count > 0)
                        {
                            onReadLine(line, header);
                        }
                        else
                        {
                            header = line;
                        }
                        count++;
                    }
                }
            }
        }
    }

}

