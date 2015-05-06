// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using DotNetNuke.Services.Localization;

namespace Dnn.DynamicContent.Exceptions
{
    public class DataTypeInUseException : InvalidOperationException
    {
        public DataTypeInUseException(DataType dataType)
            :base(String.Format(Localization.GetString("DataTypeInUse", Localization.ExceptionsResourceFile), dataType.DataTypeId))
        {
        }
    }
}
