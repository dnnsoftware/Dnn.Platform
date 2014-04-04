using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace DotNetNuke.Web.DDRMenu.DNNCommon
{
	public class PathResolver
	{
		public enum RelativeTo
		{
			Container,
			Dnn,
			Manifest,
			Module,
			Portal,
			Skin,
		}

		private readonly string manifestFolder;

		public PathResolver(string manifestFolder)
		{
			this.manifestFolder = manifestFolder;
		}

		public string Resolve(string path, params RelativeTo[] roots)
		{
			var context = DNNContext.Current;

			var mappings = new Dictionary<string, RelativeTo>
			               {
			               	{"[DDRMENU]", RelativeTo.Module},
			               	{"[MODULE]", RelativeTo.Module},
			               	{"[MANIFEST]", RelativeTo.Manifest},
			               	{"[PORTAL]", RelativeTo.Portal},
			               	{"[SKIN]", RelativeTo.Skin},
			               	{"[CONTAINER]", RelativeTo.Container},
			               	{"[DNN]", RelativeTo.Dnn}
			               };
			foreach (var key in mappings.Keys)
			{
				if (path.StartsWith(key, StringComparison.InvariantCultureIgnoreCase))
				{
					path = path.Substring(key.Length);
					roots = new[] {mappings[key]};
					break;
				}
			}

			if (path.StartsWith("/"))
			{
				path = path.Substring(1);
			}

			if (!path.StartsWith("~") && !path.Contains(":"))
			{
				foreach (var root in roots)
				{
					string resolvedPath = null;
					switch (root)
					{
						case RelativeTo.Container:
							var container = context.HostControl;
							while ((container != null) && !(container is UI.Containers.Container))
							{
								container = container.Parent;
							}
							var containerRoot = (container == null)
							                    	? context.SkinPath
// ReSharper disable PossibleNullReferenceException
													: Path.GetDirectoryName(((UI.Containers.Container)container).AppRelativeVirtualPath).Replace(
// ReSharper restore PossibleNullReferenceException
														'\\', '/') + "/";
							resolvedPath = Path.Combine(containerRoot, path);
							break;
						case RelativeTo.Dnn:
							resolvedPath = Path.Combine("~/", path);
							break;
						case RelativeTo.Manifest:
							if (!string.IsNullOrEmpty(manifestFolder))
							{
								resolvedPath = Path.Combine(manifestFolder + "/", path);
							}
							break;
						case RelativeTo.Module:
							resolvedPath = Path.Combine(DNNContext.ModuleFolder, path);
							break;
						case RelativeTo.Portal:
							resolvedPath = Path.Combine(context.PortalSettings.HomeDirectory, path);
							break;
						case RelativeTo.Skin:
							resolvedPath = Path.Combine(context.SkinPath, path);
							break;
					}

					if (!String.IsNullOrEmpty(resolvedPath))
					{
						var sep = resolvedPath.LastIndexOf('/');
						var dirName = resolvedPath.Substring(0, sep + 1);
						var fileName = resolvedPath.Substring(sep + 1);

						var mappedDir = HttpContext.Current.Server.MapPath(dirName);
						mappedDir = mappedDir.Remove(mappedDir.Length - 1);
						if (Directory.Exists(mappedDir))
						{
							if (String.IsNullOrEmpty(fileName))
								return resolvedPath.Replace('\\', '/');
	
							var matches = Directory.GetFileSystemEntries(mappedDir, fileName);
							if (matches.Length > 0)
							{
								resolvedPath = (dirName + Path.GetFileName(matches[0])).Replace('\\', '/');
								return resolvedPath;
							}
						}
					}
				}
			}

			return path;
		}
	}
}