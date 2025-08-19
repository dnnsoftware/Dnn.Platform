// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Application
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;

    using NewReleaseMode = DotNetNuke.Abstractions.Application.ReleaseMode;

    /// <inheritdoc />
    public class Application : IApplicationInfo
    {
        private static NewReleaseMode status = NewReleaseMode.None;

        /// <inheritdoc />
        public string Company => "DNN Corporation";

        /// <inheritdoc />
        public virtual Version CurrentVersion => DataProvider.Instance().GetVersion();

        /// <inheritdoc />
        public virtual string Description => "DNN Platform";

        /// <inheritdoc />
        public string HelpUrl => "https://docs.dnncommunity.org/";

        /// <inheritdoc />
        public string LegalCopyright => $"DNN Platform is copyright 2002-{DateTime.Today:yyyy} by .NET Foundation";

        /// <inheritdoc />
        public virtual string Name => "DNNCORP.CE";

        /// <inheritdoc />
        public virtual string SKU => "DNN";

        /// <summary>Gets the status of the DotnetNuke application.</summary>
        /// <remarks>If the value is not <see cref="Abstractions.Application.ReleaseMode.Stable"/>, you will see the exact status and version in page's title if allow display beta message in host setting.</remarks>
        /// <value>The value can be: None, Alpha, Beta, RC, Stable.</value>
        [Obsolete("Deprecated in DotNetNuke 9.7.0. Use 'DotNetNuke.Abstractions.Application.IApplicationInfo' with Dependency Injection instead. Scheduled removal in v11.0.0.")]
        public ReleaseMode Status { get => (ReleaseMode)(this as IApplicationInfo).Status; }

        /// <inheritdoc />
        NewReleaseMode IApplicationInfo.Status
        {
            get
            {
                if (status != NewReleaseMode.None)
                {
                    return status;
                }

                var assembly = Assembly.GetExecutingAssembly();
                if (!Attribute.IsDefined(assembly, typeof(AssemblyStatusAttribute)))
                {
                    return status;
                }

                var statusAttribute = Attribute.GetCustomAttribute(assembly, typeof(AssemblyStatusAttribute));
                if (statusAttribute != null)
                {
                    status = (NewReleaseMode)((AssemblyStatusAttribute)statusAttribute).Status;
                }

                return status;
            }
        }

        /// <inheritdoc />
        public string Title => "DotNetNuke";

        /// <inheritdoc />
        public string Trademark => "DotNetNuke,DNN";

        /// <inheritdoc />
        public string Type => "Framework";

        /// <inheritdoc />
        public string UpgradeUrl
        {
            get
            {
                var url = Config.GetSetting("UpdateServiceUrl");
                if (string.IsNullOrEmpty(url))
                {
                    return "https://dnnplatform.io";
                }

                return url;
            }
        }

        /// <inheritdoc />
        public string Url => "https://dnncommunity.org";

        /// <inheritdoc />
        public virtual Version Version
        {
            get
            {
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var fileVersion = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
                return new Version(fileVersion);
            }
        }

        /// <inheritdoc />
        public virtual bool ApplyToProduct(string productNames) => productNames.Contains(this.Name);
    }
}
