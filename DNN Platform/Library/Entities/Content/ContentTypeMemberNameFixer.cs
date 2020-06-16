// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content
{
    using System;

    /// <summary>
    /// This class exists solely to maintain compatibility between the original VB version
    /// which supported ContentType.ContentType and the c# version which doesn't allow members with
    /// the same naem as their parent type.
    /// </summary>
    [Serializable]
    public abstract class ContentTypeMemberNameFixer
    {
        public string ContentType { get; set; }
    }
}
