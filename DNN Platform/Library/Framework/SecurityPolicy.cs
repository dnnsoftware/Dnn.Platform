// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System;
    using System.Net;
    using System.Security;
    using System.Security.Permissions;
    using System.Web;

    public class SecurityPolicy
    {
        public const string ReflectionPermission = "ReflectionPermission";
        public const string WebPermission = "WebPermission";
        public const string AspNetHostingPermission = "AspNetHostingPermission";
        public const string UnManagedCodePermission = "UnManagedCodePermission";
        private static bool m_Initialized;
        private static bool m_ReflectionPermission;
        private static bool m_WebPermission;
        private static bool m_AspNetHostingPermission;
        private static bool m_UnManagedCodePermission;

        public static string Permissions
        {
            get
            {
                string strPermissions = string.Empty;
                if (HasReflectionPermission())
                {
                    strPermissions += ", " + ReflectionPermission;
                }

                if (HasWebPermission())
                {
                    strPermissions += ", " + WebPermission;
                }

                if (HasAspNetHostingPermission())
                {
                    strPermissions += ", " + AspNetHostingPermission;
                }

                if (!string.IsNullOrEmpty(strPermissions))
                {
                    strPermissions = strPermissions.Substring(2);
                }

                return strPermissions;
            }
        }

        public static bool HasAspNetHostingPermission()
        {
            GetPermissions();
            return m_AspNetHostingPermission;
        }

        public static bool HasReflectionPermission()
        {
            GetPermissions();
            return m_ReflectionPermission;
        }

        public static bool HasWebPermission()
        {
            GetPermissions();
            return m_WebPermission;
        }

        public static bool HasUnManagedCodePermission()
        {
            GetPermissions();
            return m_UnManagedCodePermission;
        }

        public static bool HasPermissions(string permissions, ref string permission)
        {
            bool _HasPermission = true;
            if (!string.IsNullOrEmpty(permissions))
            {
                foreach (string per in (permissions + ";").Split(Convert.ToChar(";")))
                {
                    if (!string.IsNullOrEmpty(per.Trim()))
                    {
                        permission = per;
                        switch (permission)
                        {
                            case AspNetHostingPermission:
                                if (HasAspNetHostingPermission() == false)
                                {
                                    _HasPermission = false;
                                }

                                break;
                            case ReflectionPermission:
                                if (HasReflectionPermission() == false)
                                {
                                    _HasPermission = false;
                                }

                                break;
                            case UnManagedCodePermission:
                                if (HasUnManagedCodePermission() == false)
                                {
                                    _HasPermission = false;
                                }

                                break;
                            case WebPermission:
                                if (HasWebPermission() == false)
                                {
                                    _HasPermission = false;
                                }

                                break;
                        }
                    }
                }
            }

            return _HasPermission;
        }

        [Obsolete("Replaced by correctly spelt method. Scheduled removal in v10.0.0.")]
        public static bool HasRelectionPermission()
        {
            GetPermissions();
            return m_ReflectionPermission;
        }

        private static void GetPermissions()
        {
            if (!m_Initialized)
            {
                // test RelectionPermission
                CodeAccessPermission securityTest;
                try
                {
                    securityTest = new ReflectionPermission(PermissionState.Unrestricted);
                    securityTest.Demand();
                    m_ReflectionPermission = true;
                }
                catch
                {
                    // code access security error
                    m_ReflectionPermission = false;
                }

                // test WebPermission
                try
                {
                    securityTest = new WebPermission(PermissionState.Unrestricted);
                    securityTest.Demand();
                    m_WebPermission = true;
                }
                catch
                {
                    // code access security error
                    m_WebPermission = false;
                }

                // test WebHosting Permission (Full Trust)
                try
                {
                    securityTest = new AspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted);
                    securityTest.Demand();
                    m_AspNetHostingPermission = true;
                }
                catch
                {
                    // code access security error
                    m_AspNetHostingPermission = false;
                }

                m_Initialized = true;

                // Test for Unmanaged Code permission
                try
                {
                    securityTest = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                    securityTest.Demand();
                    m_UnManagedCodePermission = true;
                }
                catch (Exception)
                {
                    m_UnManagedCodePermission = false;
                }
            }
        }
    }
}
