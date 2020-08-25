// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Application
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;

    /// <summary>
    /// The Application class contains properties that describe the DotNetNuke Application.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class Application
    {
        private static ReleaseMode _status = ReleaseMode.None;

        protected internal Application()
        {
        }

        /// <summary>
        /// Gets the company to which the DotNetNuke application is related.
        /// </summary>
        /// <value>Fixed result: DotNetNuke Corporation.</value>
        public string Company
        {
            get
            {
                return "DNN Corporation";
            }
        }

        /// <summary>
        /// Gets the version of the currently installed DotNetNuke framework/application
        /// Can be prior to Version, if the application is pending to be upgraded.
        /// </summary>
        /// <value>The version as retreieved from the database version table.</value>
        public virtual Version CurrentVersion
        {
            get
            {
                return DataProvider.Instance().GetVersion();
            }
        }

        /// <summary>
        /// Gets the description of the application.
        /// </summary>
        /// <value>Fixed result: DNN Platform.</value>
        public virtual string Description
        {
            get
            {
                return "DNN Platform";
            }
        }

        /// <summary>
        /// Gets the help URL related to the DotNetNuke application.
        /// </summary>
        /// <value>Fixed result: https://dnndocs.com/. </value>
        public string HelpUrl
        {
            get
            {
                return "https://dnndocs.com/";
            }
        }

        /// <summary>
        /// Gets the legal copyright.
        /// </summary>
        /// <value>Dynamic: DNN Platform is copyright 2002-todays year by .NET Foundation".</value>
        public string LegalCopyright
        {
            get
            {
                return string.Concat("DNN Platform is copyright 2002-", DateTime.Today.ToString("yyyy"), " by .NET Foundation");
            }
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <value>Fixed result: DNNCORP.CE.</value>
        public virtual string Name
        {
            get
            {
                return "DNNCORP.CE";
            }
        }

        /// <summary>
        /// Gets the SKU (Stock Keeping Unit).
        /// </summary>
        /// <value>Fixed result: DNN.</value>
        public virtual string SKU
        {
            get
            {
                return "DNN";
            }
        }

        /// <summary>
        /// Gets the status of the DotnetNuke application.
        /// </summary>
        /// <remarks>
        /// If the value is not be Stable, you will see the exactly status and version in page's title if allow display beta message in host setting.
        /// </remarks>
        /// <value>
        /// The value can be: None, Alpha, Beta, RC, Stable.
        /// </value>
        public ReleaseMode Status
        {
            get
            {
                if (_status == ReleaseMode.None)
                {
                    Assembly assy = Assembly.GetExecutingAssembly();
                    if (Attribute.IsDefined(assy, typeof(AssemblyStatusAttribute)))
                    {
                        Attribute attr = Attribute.GetCustomAttribute(assy, typeof(AssemblyStatusAttribute));
                        if (attr != null)
                        {
                            _status = ((AssemblyStatusAttribute)attr).Status;
                        }
                    }
                }

                return _status;
            }
        }

        /// <summary>
        /// Gets the title of the application.
        /// </summary>
        /// <value>Fixed value: DotNetNuke.</value>
        public string Title
        {
            get
            {
                return "DotNetNuke";
            }
        }

        /// <summary>
        /// Gets the trademark.
        /// </summary>
        /// <value>Fixed value: DotNetNuke,DNN.</value>
        public string Trademark
        {
            get
            {
                return "DotNetNuke,DNN";
            }
        }

        /// <summary>
        /// Gets the type of the application.
        /// </summary>
        /// <value>Fixed value: Framework.</value>
        public string Type
        {
            get
            {
                return "Framework";
            }
        }

        /// <summary>
        /// Gets the upgrade URL.
        /// </summary>
        /// <value>Fixed value: https://dnnplatform.io. </value>
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

        /// <summary>
        /// Gets the URL of the application.
        /// </summary>
        /// <value>Fixed value: https://dnncommunity.org.</value>
        public string Url
        {
            get
            {
                return "https://dnncommunity.org";
            }
        }

        /// <summary>
        /// Gets the version of the DotNetNuke framework/application.
        /// </summary>
        /// <value>The version as retreieved from the Executing assembly.</value>
        public virtual Version Version
        {
            get
            {
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var fileVersion = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
                return new Version(fileVersion);
            }
        }

        /// <summary>
        ///   Determine whether a product specific change is to be applied.
        /// </summary>
        /// <param name = "productNames">list of product names.</param>
        /// <returns>true if product is within list of names.</returns>
        /// <remarks>
        /// </remarks>
        public virtual bool ApplyToProduct(string productNames)
        {
            return productNames.Contains(this.Name);
        }
    }
}
