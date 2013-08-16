using System;
using System.Configuration.Provider;

namespace ClientDependency.Core.CompositeFiles.Providers
{
    public class FileMapProviderCollection : ProviderCollection
    {
        public new BaseFileMapProvider this[string name]
        {
            get { return (BaseFileMapProvider)base[name]; }
        }

        public override void Add(ProviderBase provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (!(provider is BaseFileMapProvider))
                throw new ArgumentException("Invalid provider type", "provider");

            base.Add(provider);
        }
    }
}