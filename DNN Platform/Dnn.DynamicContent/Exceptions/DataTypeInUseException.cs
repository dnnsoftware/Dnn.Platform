// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Dnn.DynamicContent.Exceptions
{
    public class DataTypeInUseException : InvalidOperationException
    {
        public DataTypeInUseException(DataType dataType)
            :base(String.Format(DotNetNuke.Services.Localization.Localization.GetString("DataTypeInUse", DotNetNuke.Services.Localization.Localization.ExceptionsResourceFile), dataType.DataTypeId))
        {
        }
    }
}
