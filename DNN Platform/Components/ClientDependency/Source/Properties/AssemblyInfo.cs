using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ClientDependency.Core")]
[assembly: AssemblyDescription("Script file and Css file management for web sites")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("ClientDependency.Core")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("acb95044-dd5c-4d0a-8523-a6352470c424")]

//ADD RESOURCES
[assembly: System.Web.UI.WebResource("ClientDependency.Core.Resources.LazyLoader.js", "text/javascript")]

[assembly: InternalsVisibleTo("ClientDependency.UnitTests")]