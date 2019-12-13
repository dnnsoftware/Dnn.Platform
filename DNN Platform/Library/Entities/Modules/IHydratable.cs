// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Data;

#endregion

namespace DotNetNuke.Entities.Modules
{
    public interface IHydratable
    {
        int KeyID { get; set; }

        void Fill(IDataReader dr);
    }
}
