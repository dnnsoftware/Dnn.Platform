// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security;

namespace Dnn.DynamicContent.Exceptions
{
    /// <summary>
    /// Global Data Type can be created, modified or deleted only by super users
    /// </summary>
    public class GlobalDataTypeSecurityException : SecurityException
    {
        public GlobalDataTypeSecurityException()
            : base(string.Format(
            DotNetNuke.Services.Localization.Localization.GetString("GlobalDataTypesSecurityException", 
            DotNetNuke.Services.Localization.Localization.ExceptionsResourceFile)))
        {
        }
    }
}
