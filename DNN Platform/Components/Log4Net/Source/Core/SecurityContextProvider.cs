using log4net.Util;

namespace log4net.Core
{
	public class SecurityContextProvider
	{
		private static SecurityContextProvider s_defaultProvider;

		public static SecurityContextProvider DefaultProvider
		{
			get
			{
				return SecurityContextProvider.s_defaultProvider;
			}
			set
			{
				SecurityContextProvider.s_defaultProvider = value;
			}
		}

		static SecurityContextProvider()
		{
			SecurityContextProvider.s_defaultProvider = new SecurityContextProvider();
		}

		protected SecurityContextProvider()
		{
		}

		public virtual SecurityContext CreateSecurityContext(object consumer)
		{
			return NullSecurityContext.Instance;
		}
	}
}