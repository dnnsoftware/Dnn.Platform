using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration.Provider;

namespace ClientDependency.Core.CompositeFiles.Providers
{
	public class CompositeFileProcessingProviderCollection : ProviderCollection
	{
		public new BaseCompositeFileProcessingProvider this[string name]
		{
			get { return (BaseCompositeFileProcessingProvider)base[name]; }
		}

		public override void Add(ProviderBase provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			if (!(provider is BaseCompositeFileProcessingProvider))
				throw new ArgumentException("Invalid provider type", "provider");

			base.Add(provider);
		}
	}
}
