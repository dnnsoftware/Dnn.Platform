﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
            if (!fullName.ToLowerInvariant().EndsWith(".csv") && !fullName.ToLowerInvariant().EndsWith(".sql"))
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

