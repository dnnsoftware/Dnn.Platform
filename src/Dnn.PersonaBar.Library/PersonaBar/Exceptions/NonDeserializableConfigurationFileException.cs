#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;

namespace Dnn.PersonaBar.Library.PersonaBar.Exceptions
{
    public class NonDeserializableConfigurationFileException : InvalidConfigurationFileException
    {
        public NonDeserializableConfigurationFileException(string absoluteFilePath, Exception e)
            : base(string.Format("Error deserilization of file: {0}", absoluteFilePath), e)
        {

        }
    }
}
