#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

#region "Usings"

using System;

#endregion

namespace DotNetNuke.Modules.PreviewProfileManagement.Components
{
	/// <summary>
	/// Event args with a primary key.
	/// </summary>
	public class PrimaryKeyEventArgs : EventArgs
	{
		#region "Public Properties"

		/// <summary>
		/// The primary key need to process by caller.
		/// </summary>
		public int Id { get; set; }

		#endregion

		#region "Constructors"

		/// <summary>
		/// Default constructor for PrimaryKeyEventArgs.
		/// </summary>
		/// <param name="id">The primary key need to process.</param>
		public PrimaryKeyEventArgs(int id)
		{
			Id = id;
		}

		#endregion
	}
}