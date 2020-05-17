﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Entities.Users
{
    [Serializable]
    public class InvalidVerificationCodeException : Exception
    {
        public InvalidVerificationCodeException()
        {
        }

        public InvalidVerificationCodeException(string message)
            : base(message)
        {
        }

        public InvalidVerificationCodeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public InvalidVerificationCodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
