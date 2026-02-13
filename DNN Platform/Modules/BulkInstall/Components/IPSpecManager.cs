// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using Dnn.Modules.BulkInstall.Components.DataAccess.DataControllers;
    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;
    using Dnn.Modules.BulkInstall.Components.Exceptions;

    /// <summary>A manager for <see cref="IPSpec"/>.</summary>
    /// <param name="dataController">The data controller.</param>
    public sealed class IPSpecManager(IPSpecDataController dataController)
    {
        private readonly IPSpecDataController dataController = dataController;

        /// <summary>Create a new IPSpec object using the passed name and address.</summary>
        /// <param name="name">The label.</param>
        /// <param name="address">The IP address.</param>
        /// <returns>The new <see cref="IPSpec"/>.</returns>
        public IPSpec Create(string name, string address)
        {
            IPSpec ipSpec = this.dataController.GetByName(name);

            if (ipSpec != null)
            {
                throw new IPSpecExistsException($"An entry named '{ipSpec.Name}' already exists.");
            }

            ipSpec = this.dataController.Get(address);

            if (ipSpec != null)
            {
                throw new IPSpecExistsException($"IP '{address}' is already whitelisted by entry named '{ipSpec.Name}'.");
            }

            ipSpec = new IPSpec(name, address);

            this.dataController.Create(ipSpec);

            return ipSpec;
        }

        /// <summary>Retrieve all the <see cref="IPSpec"/> objects from the database.</summary>
        /// <returns>A sequence of <see cref="IPSpec"/>.</returns>
        public IEnumerable<IPSpec> GetAll()
        {
            return this.dataController.Get();
        }

        /// <summary>Gets a single <see cref="IPSpec"/> by its ID.</summary>
        /// <param name="id">The IP spec ID.</param>
        /// <returns>The <see cref="IPSpec"/> or <see langword="null"/>.</returns>
        public IPSpec GetById(int id)
        {
            return this.dataController.Get(id);
        }

        /// <summary>Check to see if the passed address is allowed.</summary>
        /// <param name="address">The IP address.</param>
        /// <returns><see langword="true"/> if the address is allowed, otherwise <see langword="false"/>.</returns>
        public bool IsAllowed(string address)
        {
            if (!IPAddress.TryParse(address, out _))
            {
                // see if address is an IP plus port, e.g. "1.1.1.1:58290"
                if (Uri.TryCreate(Uri.UriSchemeHttps + Uri.SchemeDelimiter + address, UriKind.Absolute, out Uri uri))
                {
                    if (uri.HostNameType != UriHostNameType.IPv4 && uri.HostNameType != UriHostNameType.IPv6)
                    {
                        return false;
                    }

                    address = uri.Host;
                }
            }

            IPSpec ipSpec = this.dataController.Get(address);

            return ipSpec != null;
        }

        /// <summary>Delete the passed <see cref="IPSpec"/>.</summary>
        /// <param name="ipSpec">The IP spec to delete.</param>
        public void Delete(IPSpec ipSpec)
        {
            this.dataController.Delete(ipSpec);
        }
    }
}
