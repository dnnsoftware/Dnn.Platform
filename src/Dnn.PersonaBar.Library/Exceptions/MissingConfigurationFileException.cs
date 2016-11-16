#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;

namespace Dnn.PersonaBar.Library.Exceptions
{
    public class MissingConfigurationFileException : Exception
    {
        public MissingConfigurationFileException(string absoluteFilePath, Exception e)
            : base(string.Format("Error reading the file: {0}", absoluteFilePath), e)
        {

        }
    }
}
