#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;

namespace Dnn.PersonaBar.Library.Exceptions
{
    public class InvalidConfigurationFileException : Exception
    {
        public InvalidConfigurationFileException(string message, Exception e)
            : base(message, e)
        {

        }
    }
}
