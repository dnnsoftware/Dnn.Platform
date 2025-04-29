// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework;

using System;
using System.Net;
using System.Security;
using System.Security.Permissions;
using System.Web;

using DotNetNuke.Internal.SourceGenerators;

public partial class SecurityPolicy
{
    public const string ReflectionPermission = "ReflectionPermission";
    public const string WebPermission = "WebPermission";
    public const string AspNetHostingPermission = "AspNetHostingPermission";
    public const string UnManagedCodePermission = "UnManagedCodePermission";
    private static bool initialized;
    private static bool reflectionPermission;
    private static bool webPermission;
    private static bool aspNetHostingPermission;
    private static bool unManagedCodePermission;

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
        return aspNetHostingPermission;
    }

    public static bool HasReflectionPermission()
    {
        GetPermissions();
        return reflectionPermission;
    }

    public static bool HasWebPermission()
    {
        GetPermissions();
        return webPermission;
    }

    public static bool HasUnManagedCodePermission()
    {
        GetPermissions();
        return unManagedCodePermission;
    }

    public static bool HasPermissions(string permissions, ref string permission)
    {
        bool hasPermission = true;
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
                                hasPermission = false;
                            }

                            break;
                        case ReflectionPermission:
                            if (HasReflectionPermission() == false)
                            {
                                hasPermission = false;
                            }

                            break;
                        case UnManagedCodePermission:
                            if (HasUnManagedCodePermission() == false)
                            {
                                hasPermission = false;
                            }

                            break;
                        case WebPermission:
                            if (HasWebPermission() == false)
                            {
                                hasPermission = false;
                            }

                            break;
                    }
                }
            }
        }

        return hasPermission;
    }

    [DnnDeprecated(7, 0, 0, "Replaced by correctly spelt method", RemovalVersion = 10)]
    public static partial bool HasRelectionPermission()
    {
        GetPermissions();
        return reflectionPermission;
    }

    private static void GetPermissions()
    {
        if (!initialized)
        {
            // test RelectionPermission
            CodeAccessPermission securityTest;
            try
            {
                securityTest = new ReflectionPermission(PermissionState.Unrestricted);
                securityTest.Demand();
                reflectionPermission = true;
            }
            catch
            {
                // code access security error
                reflectionPermission = false;
            }

            // test WebPermission
            try
            {
                securityTest = new WebPermission(PermissionState.Unrestricted);
                securityTest.Demand();
                webPermission = true;
            }
            catch
            {
                // code access security error
                webPermission = false;
            }

            // test WebHosting Permission (Full Trust)
            try
            {
                securityTest = new AspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted);
                securityTest.Demand();
                aspNetHostingPermission = true;
            }
            catch
            {
                // code access security error
                aspNetHostingPermission = false;
            }

            initialized = true;

            // Test for Unmanaged Code permission
            try
            {
                securityTest = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                securityTest.Demand();
                unManagedCodePermission = true;
            }
            catch (Exception)
            {
                unManagedCodePermission = false;
            }
        }
    }
}
