// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.ComponentModel.Helpers
{
    public class Service2Impl : IService2
    {
        private readonly IService _service;

        public Service2Impl(IService service)
        {
            this._service = service;
        }

        public IService Service
        {
            get { return this._service; }
        }
    }
}
