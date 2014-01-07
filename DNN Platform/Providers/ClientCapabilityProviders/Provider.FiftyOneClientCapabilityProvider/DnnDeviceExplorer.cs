#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

namespace DotNetNuke.Providers.FiftyOneClientCapabilityProvider
{
    using System;
    using System.Collections.Specialized;
    using System.Web.UI.WebControls;
    using Common;

    /// <summary>
    /// Inherits from DeviceExplorer, overrides URL generation methods and UI building methods to add DNN specific functionality.
    /// </summary>
    public class DnnDeviceExplorer : FiftyOne.Foundation.UI.Web.DeviceExplorer
    {
        /// <summary>
        /// Revises current page URL with a specific key/value parameter.
        /// </summary>
        /// <param name="key">The key to be added to the querystring.</param>
        /// <param name="value">The value to be added to the querystring.</param>
        /// <returns>The fully qualified URL.</returns>
        protected override string GetNewUrl(string key, string value)
        {
            return GetNewUrl(new NameValueCollection(Request.QueryString), key, value);
        }

        /// <summary>
        /// Revises current page URL with a specific key/value parameter.
        /// </summary>
        /// <param name="parameters">The current query string parameters. Used for </param>
        /// <param name="key">The key to be added to the querystring.</param>
        /// <param name="value">The value to be added to the querystring.</param>
        /// <returns>The fully qualified URL.</returns>
        protected override string GetNewUrl(NameValueCollection parameters, string key, string value)
        {
            var tabIdSetting = parameters["TabID"];
            int tabId;
            
            if (int.TryParse(tabIdSetting, out tabId))
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                {
                    return Globals.NavigateURL(tabId);
                }

                var deviceIdKey = parameters["DeviceID"] != null && key != "DeviceID" ? "DeviceID=" + parameters["DeviceID"] : string.Empty;
                string vendorKey = string.Empty;
                string modelKey = string.Empty;

                // only set vendor key and model if device ID is not set
                if (deviceIdKey == string.Empty && key != "DeviceID")
                {
                    vendorKey = parameters["Vendor"] != null && key != "Vendor" ? "Vendor=" + parameters["Vendor"] : string.Empty;
                    modelKey = parameters["Model"] != null && key != "Model" ? "Model=" + parameters["Model"] : string.Empty;
                }

                var queryKey = parameters["Query"] != null ? "Query=" + parameters["Query"] : string.Empty;

                return Globals.NavigateURL(tabId, string.Empty, queryKey, key + "=" + value, deviceIdKey, modelKey, vendorKey);
            }

            return string.Empty;
        }
        
        /// <summary>
        /// Builds a label (conditionally using the dnnTooltip) and adds it to the provided panel.
        /// </summary>
        /// <param name="panel">The panel to add label to.</param>
        /// <param name="text">The text of the label.</param>
        /// <param name="tooltip">The value of the tooltip.</param>
        /// <param name="url">The URL to be included in the tooltip. Presence dictates that the tooltip a link.</param>
        /// <param name="anchor">The name of the anchor.</param>
        protected override void AddLabel(WebControl panel, string text, string tooltip, Uri url, string anchor)
        {
            var toolTip = new Literal();

//            if (!string.IsNullOrEmpty(tooltip))
//            {
////                const string toolTipHtml =
////                    @"<div class=""dnnTooltip"">
////                        <label>
////                            <a class=""dnnFormHelp"" href=""#"">{0}</a>
////                        </label>
////                        <div class=""dnnFormHelpContent dnnClear"" style=""display: none;"">
////                            <span class=""dnnHelpText"">{1}</span>
////                            <a href=""#"" class=""pinHelp""></a>
////                        </div>
////                      </div>";

//                const string toolTipHtml =
//                    @"<div class=""dnnTooltip"">
//                              <div class=""dnnFormHelpContent dnnClear"">
//                                <span class=""dnnHelpText"">{1}</span>
//                                <a href=""#"" class=""pinHelp""></a>
//                            </div>   
//                      </div>      
//                    ";

//                string link = url != null ? string.Format("<a href=\"{0}\">{1}</a>", url, tooltip) : tooltip;
//                toolTip.Text = string.Format(toolTipHtml, text, link);
//            }
//            else
            {
                toolTip.Text = string.Format("<span>{0}</span>", text);
            }

            panel.Controls.Add(toolTip);
        }
    }
}