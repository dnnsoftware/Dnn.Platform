#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using DotNetNuke.Data;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AssemblyInstaller installs Assembly Components to a DotNetNuke site
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	07/24/2007  created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class AssemblyInstaller : FileInstaller
    {
		#region "Protected Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("assemblies")
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "assemblies";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the default Path for the file - if not present in the manifest
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	08/10/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override string DefaultPath
        {
            get
            {
                return "bin\\";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("assembly")
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "assembly";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the PhysicalBasePath for the assemblies
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	07/25/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override string PhysicalBasePath
        {
            get
            {
                return PhysicalSitePath + "\\";
            }
        }
		
		#endregion

		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List)
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	03/28/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get
            {
                return "dll";
            }
        }
		
		
		#endregion

		#region "Protected Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteFile method deletes a single assembly.
        /// </summary>
        /// <param name="file">The InstallFile to delete</param>
        /// <history>
        /// 	[cnurse]	08/01/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void DeleteFile(InstallFile file)
        {
            //Attempt to unregister assembly this will return False if the assembly is used by another package and
            //cannot be delete andtrue if it is not being used and can be deleted
            if (DataProvider.Instance().UnRegisterAssembly(Package.PackageID, file.Name))
            {
                Log.AddInfo(Util.ASSEMBLY_UnRegistered + " - " + file.FullName);
                //Call base class version to deleteFile file from \bin
                base.DeleteFile(file);
            }
            else
            {
                Log.AddInfo(Util.ASSEMBLY_InUse + " - " + file.FullName);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a flag that determines what type of file this installer supports
        /// </summary>
        /// <param name="type">The type of file being processed</param>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override bool IsCorrectType(InstallFileType type)
        {
            return (type == InstallFileType.Assembly);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The InstallFile method installs a single assembly.
        /// </summary>
        /// <param name="file">The InstallFile to install</param>
        /// <history>
        /// 	[cnurse]	08/01/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override bool InstallFile(InstallFile file)
        {
            bool bSuccess = true;
            if (file.Action == "UnRegister")
            {
                DeleteFile(file);
            }
            else
            {
                //Attempt to register assembly this will return False if the assembly exists and true if it does not or is older
                int returnCode = DataProvider.Instance().RegisterAssembly(Package.PackageID, file.Name, file.Version.ToString(3));
                switch (returnCode)
                {
                    case 0:
                        //Assembly Does Not Exist
                        Log.AddInfo(Util.ASSEMBLY_Added + " - " + file.FullName);
                        break;
                    case 1:
                        //Older version of Assembly Exists
                        Log.AddInfo(Util.ASSEMBLY_Updated + " - " + file.FullName);
                        break;
                    case 2:
                    case 3:
						//Assembly already Registered
                        Log.AddInfo(Util.ASSEMBLY_Registered + " - " + file.FullName);
                        break;
                }
				
                //If assembly not registered, is newer (or is the same version and we are in repair mode)
                if (returnCode < 2 || (returnCode == 2 && file.InstallerInfo.RepairInstall))
                {
                    //Call base class version to copy file to \bin
					bSuccess = base.InstallFile(file);
                }
            }
            return bSuccess;
        }
		
		#endregion
    }
}
