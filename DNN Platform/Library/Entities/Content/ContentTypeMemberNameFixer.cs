// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Entities.Content
{
    /// <summary>
    /// This class exists solely to maintain compatibility between the original VB version
    /// which supported ContentType.ContentType and the c# version which doesn't allow members with
    /// the same naem as their parent type
    /// </summary>
    [Serializable]
    public abstract class ContentTypeMemberNameFixer
    {
        public string ContentType { get; set; }
    }
}
