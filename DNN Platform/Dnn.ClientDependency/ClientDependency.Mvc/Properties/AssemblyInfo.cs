using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ClientDependency.Mvc")]
[assembly: AssemblyDescription("An extension for the ClientDependency framework to support ASP.Net MVC")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("ClientDependency.Mvc")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("edff80c4-ff38-4468-9f25-088e4c54ebbb")]

#if Debug || Release
[assembly: AllowPartiallyTrustedCallers]
#endif