// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework
{
    public class FileParameter
    {
        public FileParameter(byte[] file)
            : this(file, null)
        {
        }

        public FileParameter(byte[] file, string filename)
            : this(file, filename, null)
        {
        }

        public FileParameter(byte[] file, string filename, string contenttype)
        {
            this.File = file;
            this.FileName = filename;
            this.ContentType = contenttype;
        }

        public byte[] File { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }
    }
}
