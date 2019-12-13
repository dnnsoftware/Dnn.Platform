#region Usings

using System;

#endregion

namespace DotNetNuke.Services.Mobile
{
	public interface IMatchRule
	{
		/// <summary>
		/// Primary Id.
		/// </summary>
		int Id { get; }
		/// <summary>
		/// capbility name.
		/// </summary>
		string Capability { get; set; }

		/// <summary>
		/// reg expression to match the request
		/// </summary>
		string Expression { get; set; }
	}
}
