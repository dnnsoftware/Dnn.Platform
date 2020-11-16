// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client
{
    /// <summary>
    /// Contains enumerations that define the relative loading order of both JavaScript and CSS files within the framework's registration system.
    /// </summary>
    public class FileOrder
    {
        /// <summary>
        /// Defines load order of key JavaScript files within the framework.
        /// </summary>
        public enum Js
        {
            /// <summary>
            /// The default priority (100) indicates that the ordering will be done based on the order in which the registrations are made
            /// </summary>
            DefaultPriority = 100,

            /// <summary>
            /// jQuery (CDN or local file) has the priority of 5
            /// </summary>
            // ReSharper disable InconsistentNaming
            jQuery = 5,

            // ReSharper restore InconsistentNaming

            /// <summary>
            /// jQuery Migrate file has the priority of 6, it should appear just after jquery.
            /// </summary>
            // ReSharper disable InconsistentNaming
            jQueryMigrate = 6,

            // ReSharper restore InconsistentNaming

            /// <summary>
            /// jQuery UI (CDN or local file) has the priority of 10
            /// </summary>
// ReSharper disable InconsistentNaming
            jQueryUI = 10,

            // ReSharper restore InconsistentNaming

            /// <summary>
            /// /js/dnn.xml.js has the priority of 15
            /// </summary>
            DnnXml = 15,

            /// <summary>
            /// /js/dnn.xml.jsparser.js has the priority of 20
            /// </summary>
            DnnXmlJsParser = 20,

            /// <summary>
            /// /js/dnn.xmlhttp.js has the priority of 25
            /// </summary>
            DnnXmlHttp = 25,

            /// <summary>
            /// /js/dnn.xmlhttp.jsxmlhttprequest.js has the pririty of 30
            /// </summary>
            DnnXmlHttpJsXmlHttpRequest = 30,

            /// <summary>
            /// /js/dnn.dom.positioning.js has the priority of 35
            /// </summary>
            DnnDomPositioning = 35,

            /// <summary>
            /// /js/dnn.controls.js has the priority of 40
            /// </summary>
            DnnControls = 40,

            /// <summary>
            /// /js/dnn.controls.labeledit.js has the priority of 45
            /// </summary>
            DnnControlsLabelEdit = 45,

            /// <summary>
            /// /js/dnn.modalpopup.js has the priority of 50
            /// </summary>
            DnnModalPopup = 50,

            /// <summary>
            /// jQuery Hover Intent JS File has the priority of 55
            /// </summary>
            HoverIntent = 55,
        }

        /// <summary>
        /// Defines load order of key CSS files within the framework.
        /// </summary>
        public enum Css
        {
            /// <summary>
            /// The default priority (100) indicates that the ordering will be done based on the order in which the registrations are made
            /// </summary>
            DefaultPriority = 100,

            /// <summary>
            /// The default.css file has a priority of 5
            /// </summary>
            DefaultCss = 5,

            /// <summary>
            /// The admin.css file has a priority of 6
            /// </summary>
            AdminCss = 6,

            /// <summary>
            /// The feature.css file has a priority of 7
            /// </summary>
            FeatureCss = 7,

            /// <summary>
            /// The ie.css file has a priority of 8
            /// </summary>
            IeCss = 8,

            /// <summary>
            /// Module CSS files have a priority of 10
            /// </summary>
            ModuleCss = 10,

            /// <summary>
            /// Resources CSS files have a priority of 12
            /// </summary>
            ResourceCss = 12,

            /// <summary>
            /// Skin CSS files have a priority of 15
            /// </summary>
            SkinCss = 15,

            /// <summary>
            /// Specific skin control's CSS files have a priority of 20
            /// </summary>
            SpecificSkinCss = 20,

            /// <summary>
            /// Container CSS files have a priority of 25
            /// </summary>
            ContainerCss = 25,

            /// <summary>
            /// Specific container control's CSS files have a priority of 30
            /// </summary>
            SpecificContainerCss = 30,

            /// <summary>
            /// The portal.css file has a priority of 35
            /// </summary>
            PortalCss = 35,
        }
    }
}
