#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
#region Usings

using System;
using System.Reflection;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;

#endregion

namespace DotNetNuke.Application
{
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
        /// <value>Fixed result: DotNetNuke Corporation</value>
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
        /// Gets the description of the application
        /// </summary>
        /// <value>Fixed result: DotNetNuke Community Edition</value>
        public virtual string Description
        {
            get
            {
                return "DNN Platform";
            }
        }

        /// <summary>
        /// Gets the help URL related to the DotNetNuke application
        /// </summary>
        /// <value>Fixed result: http://www.dotnetnuke.com/default.aspx?tabid=787 </value>
        public string HelpUrl
        {
            get
            {
                return "http://www.dotnetnuke.com/default.aspx?tabid=787";
            }
        }

        /// <summary>
        /// Gets the legal copyright.
        /// </summary>
        /// <value>Dynamic: DotNetNuke® is copyright 2002-todays year by DotNetNuke Corporation"</value>
        public string LegalCopyright
        {
            get
            {
                return string.Concat("DotNetNuke® is copyright 2002-", DateTime.Today.ToString("yyyy")," by DotNetNuke Corporation");
            }
        }

        /// <summary>
        /// Gets the name of the application
        /// </summary>
        /// <value>Fixed result: DNNCORP.CE</value>
        public virtual string Name
        {
            get
            {
                return "DNNCORP.CE";
            }
        }

        /// <summary>
        /// Gets the SKU (Stock Keeping Unit)
        /// </summary>
        /// <value>Fixed result: DNN</value>
        public virtual string SKU
        {
            get
            {
                return "DNN";
            }
        }

        /// <summary>
        /// Gets the status of the DotnetNuke application
        /// </summary>
        /// <remarks>
		/// If the value is not be Stable, you will see the exactly status and version in page's title if allow display beta message in host setting.
        /// </remarks>
        /// <value>
        /// The value can be: None, Alpha, Beta, RC, Stable
        /// </value>
        public ReleaseMode Status
        {
            get
            {
                if (_status == ReleaseMode.None)
                {
                    Assembly assy = Assembly.GetExecutingAssembly();
                    if (Attribute.IsDefined(assy, typeof (AssemblyStatusAttribute)))
                    {
                        Attribute attr = Attribute.GetCustomAttribute(assy, typeof (AssemblyStatusAttribute));
                        if (attr != null)
                        {
                            _status = ((AssemblyStatusAttribute) attr).Status;
                        }
                    }
                }
                return _status;
            }
        }

        /// <summary>
        /// Gets the title of the application
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
        /// <value>Fixed value: DotNetNuke,DNN</value>
        public string Trademark
        {
            get
            {
                return "DotNetNuke,DNN";
            }
        }

        /// <summary>
        /// Gets the type of the application
        /// </summary>
        /// <value>Fixed value: Framework</value>
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
        /// <value>Fixed value: http://update.dotnetnuke.com </value>
        public string UpgradeUrl
        {
            get
            {
	            var url = Config.GetSetting("UpdateServiceUrl");
				if (string.IsNullOrEmpty(url))
				{
					return "http://update.dotnetnuke.com";
				}
	            return url;
            }
        }

        /// <summary>
        /// Gets the URL of the application
        /// </summary>
        /// <value>Fixed value: http://www.dotnetnuke.com </value>
        public string Url
        {
            get
            {
                return "http://www.dotnetnuke.com";
            }
        }

        /// <summary>
        /// Gets the version of the DotNetNuke framework/application
        /// </summary>
        /// <value>The version as retreieved from the Executing assembly.</value>
        public virtual Version Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        #region "Public Functions"

        /// <summary>
        ///   Determine whether a product specific change is to be applied
        /// </summary>
        /// <param name = "productNames">list of product names</param>
        /// <returns>true if product is within list of names</returns>
        /// <remarks>
        /// </remarks>
        public virtual bool ApplyToProduct(string productNames)
        {
            return productNames.Contains(Name);
        }

        #endregion
    }
}
