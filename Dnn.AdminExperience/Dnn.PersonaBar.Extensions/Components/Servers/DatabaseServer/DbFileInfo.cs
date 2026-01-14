// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Components.DatabaseServer
{
    using System;

    public class DbFileInfo
    {
        public decimal Megabytes => Convert.ToDecimal(this.Size / 1024);

        public string ShortFileName
        {
            get
            {
                if (this.FileName.IndexOf('\\') == this.FileName.LastIndexOf('\\'))
                {
                    return this.FileName;
                }

                var root = this.FileName.Substring(0, this.FileName.IndexOf('\\') + 1);
                var leaf = this.FileName.Substring(this.FileName.LastIndexOf('\\', this.FileName.LastIndexOf('\\') - 1));
                return $"{root}...{leaf}";
            }
        }

        public string FileType { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public string FileName { get; set; }
    }
}
