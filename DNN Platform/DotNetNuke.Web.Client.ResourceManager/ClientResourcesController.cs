// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Client.ResourceManager
{
    using System.Collections.Generic;
    using DotNetNuke.Abstractions.ClientResources;

    /// <inheritdoc />
    public class ClientResourcesController : IClientResourcesController
    {
        private List<ILinkResource> Links { get; set; } = new List<ILinkResource>();
        private List<IScriptResource> Scripts { get; set; } = new List<IScriptResource>();

        /// <inheritdoc />
        public void AddLink(ILinkResource link)
        {
            Links.Add(link);
        }

        /// <inheritdoc />
        public void AddScript(IScriptResource script)
        {
            Scripts.Add(script);
        }

        /// <inheritdoc />
        public ILinkResource CreateLink()
        {
            return new Models.LinkResource(this);
        }

        /// <inheritdoc />
        public IScriptResource CreateScript()
        {
            return new Models.ScriptResource(this);
        }

        /// <inheritdoc />
        public void RegisterLink(string linkPath)
        {
            this.CreateLink().FromSrc(linkPath).Register();
        }

        /// <inheritdoc />
        public void RegisterScript(string scriptPath)
        {
            this.CreateScript().FromSrc(scriptPath).Register();
        }

        /// <inheritdoc />
        public void RemoveLinkByName(string linkName)
        {
            this.Links.RemoveAll(l => l.Name == linkName);
        }

        /// <inheritdoc />
        public void RemoveLinkByPath(string linkPath)
        {
            this.Links.RemoveAll(l => l.FilePath == linkPath);
        }

        /// <inheritdoc />
        public void RemoveScriptByName(string scriptName)
        {
            this.Scripts.RemoveAll(s => s.Name == scriptName);
        }

        /// <inheritdoc />
        public void RemoveScriptByPath(string scriptPath)
        {
            this.Scripts.RemoveAll(s => s.FilePath == scriptPath);
        }
    }
}
