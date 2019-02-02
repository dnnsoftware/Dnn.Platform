﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace ClientDependency.Core.Controls
{
	
	/// <summary>
	/// A path object for the client dependency loader. Used to specify all of the base paths (name and path) to 
	/// be used with the client dependencies.
	/// Databinding support has been enabled.
	/// </summary>
	[ParseChildren(true)]
	public class ClientDependencyPath : IClientDependencyPath
	{
		string _name = "";
		string _path = "";

		public string Name { get { return _name; } set { _name = value; } }
		public string Path { get { return _path; } set { _path = value; } }

		public bool ForceBundle { get; set; }

		#region Logic to allow for databinding non-UI ASP.Net control
		public event EventHandler DataBinding;
		public void DataBind()
		{
			OnDataBinding(new EventArgs());
		}
		protected void OnDataBinding(EventArgs e)
		{
			if (DataBinding != null)
				DataBinding(this, e);
		}
		public Control BindingContainer
		{
			get
			{
				return Parent;
			}
		} 
		#endregion

		/// <summary>
		/// This is set at runtime to set the load for this path object. this is required for databinding.
		/// </summary>
		public ClientDependencyLoader Parent { get; internal set; }

		public string ResolvedPath
		{
			get
			{
				if (string.IsNullOrEmpty(Path))
					throw new ArgumentNullException("Path has not been set");
				return Parent.ResolveUrl(Path);
			}
		}

        protected bool Equals(ClientDependencyPath other)
        {
            return string.Equals(_name, other._name, StringComparison.InvariantCultureIgnoreCase)
                   && string.Equals(_path, other._path, StringComparison.InvariantCultureIgnoreCase);
        }

	    public override bool Equals(object obj)
	    {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != this.GetType()) return false;
	        return Equals((ClientDependencyPath) obj);
	    }

	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            return ((_name != null ? _name.GetHashCode() : 0)*397) ^ (_path != null ? _path.GetHashCode() : 0);
	        }
	    }
	}
}
