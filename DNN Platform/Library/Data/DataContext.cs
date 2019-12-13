#region Usings

using System;
using System.Collections.Generic;
using System.Configuration;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.UI.Containers;
using PetaPoco;

#endregion

namespace DotNetNuke.Data
{
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
