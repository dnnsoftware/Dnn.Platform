using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ClientDependency.Core")]
[assembly: AssemblyDescription("JavaScript and CSS file management for ASP.Net web sites")]
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

[assembly: AllowPartiallyTrustedCallers]

//[assembly: InternalsVisibleTo("ClientDependency.UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100275faf5f8f9b5cf529c398bdefd7cbcd8edf097ba7f1bea82ec2442f2448c54aa1797fdd3fa6097732dc6c7cc74f29f3222f548cc83a07cb07368f005f0e6b756424a9b60a560117ca57e9a4d95d5d4b1e48f78cb813ec35f44774fcfb3f715fa5c8a7881d81e3aa04a9fdca2ade7c4079c87d8694b3aabf61f179e7bf93d6c0")]
//[assembly: InternalsVisibleTo("ClientDependency.Core.Mvc, PublicKey=0024000004800000940000000602000000240000525341310004000001000100275faf5f8f9b5cf529c398bdefd7cbcd8edf097ba7f1bea82ec2442f2448c54aa1797fdd3fa6097732dc6c7cc74f29f3222f548cc83a07cb07368f005f0e6b756424a9b60a560117ca57e9a4d95d5d4b1e48f78cb813ec35f44774fcfb3f715fa5c8a7881d81e3aa04a9fdca2ade7c4079c87d8694b3aabf61f179e7bf93d6c0")]
[assembly: InternalsVisibleTo("ClientDependency.UnitTests")]
[assembly: InternalsVisibleTo("ClientDependency.Core.Mvc")]
[assembly: InternalsVisibleTo("ClientDependency.Less")]
[assembly: InternalsVisibleTo("ClientDependency.SASS")]
[assembly: InternalsVisibleTo("ClientDependency.Coffee")]
[assembly: InternalsVisibleTo("ClientDependency.TypeScript")]