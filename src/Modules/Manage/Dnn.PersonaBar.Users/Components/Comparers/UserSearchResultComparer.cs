// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2016, DNN Corp.
// by DNN Corporation
// All Rights Reserved

using System;
using System.Collections.Generic;
using DotNetNuke.Services.Search.Entities;

namespace Dnn.PersonaBar.Users.Components.Comparers
{
    public class UserSearchResultComparer : IEqualityComparer<SearchResult>
    {
        public bool Equals(SearchResult x, SearchResult y)
        {

            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.UniqueKey.Split('_')[0] == y.UniqueKey.Split('_')[0];
        }

        public int GetHashCode(SearchResult result)
        {
            //Check whether the object is null
            if (ReferenceEquals(result, null)) return 0;

            return result.UniqueKey.Split('_')[0].GetHashCode();
        }

    }

}
