// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.UI.Containers;
    using global::PetaPoco;

    public class DataContext
    {
        public static IDataContext Instance()
        {
            IDataContext instance = ComponentFactory.GetComponent<IDataContext>();

            if (instance == null)
            {
                var defaultConnectionStringName = DataProvider.Instance().Settings["connectionStringName"];

                instance = new PetaPocoDataContext(defaultConnectionStringName, DataProvider.Instance().ObjectQualifier);
            }

            return instance;
        }

        public static IDataContext Instance(string connectionStringName)
        {
            IDataContext instance = ComponentFactory.GetComponent<IDataContext>(connectionStringName);

            if (instance == null)
            {
                instance = new PetaPocoDataContext(connectionStringName, DataProvider.Instance().ObjectQualifier);
            }

            return instance;
        }

        public static IDataContext Instance(Dictionary<Type, IMapper> mappers)
        {
            IDataContext instance = ComponentFactory.GetComponent<IDataContext>();

            if (instance == null)
            {
                var defaultConnectionStringName = DataProvider.Instance().Settings["connectionStringName"];

                instance = new PetaPocoDataContext(defaultConnectionStringName, DataProvider.Instance().ObjectQualifier, mappers);
            }

            return instance;
        }

        public static IDataContext Instance(string connectionStringName, Dictionary<Type, IMapper> mappers)
        {
            IDataContext instance = ComponentFactory.GetComponent<IDataContext>(connectionStringName);

            if (instance == null)
            {
                instance = new PetaPocoDataContext(connectionStringName, DataProvider.Instance().ObjectQualifier, mappers);
            }

            return instance;
        }
    }
}
