// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.Cdf
{
    using System.Collections.Generic;

    internal class BasicFile
    {
        public BasicFile(ClientDependencyType type)
        {
            this.DependencyType = type;
            this.HtmlAttributes = new Dictionary<string, string>();
            this.Priority = 100;
            this.Group = 100;
            this.Name = string.Empty;
            this.Version = string.Empty;
            this.ForceVersion = false;
        }

        public string FilePath { get; set; }

        public ClientDependencyType DependencyType { get; private set; }

        public int Priority { get; set; }

        public int Group { get; set; }

        public string PathNameAlias { get; set; }

        public string ForceProvider { get; set; }

        public IDictionary<string, string> HtmlAttributes { get; private set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public bool ForceVersion { get; set; }
    }
}
