using System;

namespace log4net.Core
{
	[Flags]
	public enum FixFlags
	{
		None = 0,
		[Obsolete("Replaced by composite Properties")]
		Mdc = 1,
		Ndc = 2,
		Message = 4,
		ThreadName = 8,
		LocationInfo = 16,
		UserName = 32,
		Domain = 64,
		Identity = 128,
		Exception = 256,
		Properties = 512,
		Partial = 844,
		All = 268435455
	}
}