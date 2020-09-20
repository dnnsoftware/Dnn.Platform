using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;

namespace DNNConnect.CKEditorProvider.Utilities
{
    /// <summary>
    /// Utility Class for various helper Functions
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Path Validator Expression
        /// </summary>
        private static readonly string PathValidatorExpression = string.Format(
            "^[^{0}]+$",
            string.Join(string.Empty, Array.ConvertAll(Path.GetInvalidPathChars(), x => Regex.Escape(x.ToString()))));

        /// <summary>
        /// Path Validator Regex
        /// </summary>
        private static readonly Regex PathValidator = new Regex(PathValidatorExpression, RegexOptions.Compiled);

        /// <summary>
        /// FileName Validator Expression
        /// </summary>
        private static readonly string FileNameValidatorExpression = string.Format(
            "^[^{0}]+$",
            string.Join(string.Empty, Array.ConvertAll(Path.GetInvalidFileNameChars(), x => Regex.Escape(x.ToString()))));

        /// <summary>
        /// FileName Validator Regex
        /// </summary>
        private static readonly Regex FileNameValidator = new Regex(FileNameValidatorExpression, RegexOptions.Compiled);

        /// <summary>
        /// Path Cleaner Expression
        /// </summary>
        private static readonly string PathCleanerExpression = string.Format(
            "[{0}]",
            string.Join(string.Empty, Array.ConvertAll(Path.GetInvalidPathChars(), x => Regex.Escape(x.ToString()))));

        /// <summary>
        /// Path Cleaner Regex
        /// </summary>
        private static readonly Regex PathCleaner = new Regex(PathCleanerExpression, RegexOptions.Compiled);

        /// <summary>
        /// FileName Cleaner Expression
        /// </summary>
        private static readonly string FileNameCleanerExpression = string.Format(
            "[{0}]",
            string.Join(string.Empty, Array.ConvertAll(Path.GetInvalidFileNameChars(), x => Regex.Escape(x.ToString()))));

        /// <summary>
        /// FileName Cleaner Regex
        /// </summary>
        private static readonly Regex FileNameCleaner = new Regex(FileNameCleanerExpression, RegexOptions.Compiled);

        /// <summary>
        /// Check if Object is a Unit
        /// </summary>
        /// <param name="valueToCheck">
        /// Object to Check
        /// </param>
        /// <returns>
        /// Returns if Object is a Unit
        /// </returns>
        public static bool IsUnit(object valueToCheck)
        {
            string sInputValue = Convert.ToString(valueToCheck);

            string sNewValue;

            if (sInputValue.EndsWith("px"))
            {
                sNewValue = sInputValue.Replace("px", string.Empty);

                if (IsNumeric(sNewValue))
                {
                    return true;
                }
            }
            else if (sInputValue.EndsWith("%"))
            {
                sNewValue = sInputValue.Replace("%", string.Empty);

                if (IsNumeric(sNewValue))
                {
                    return true;
                }
            }
            else
            {
                if (IsNumeric(sInputValue))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if Object is a Number
        /// </summary>
        /// <param name="valueToCheck">
        /// Object to Check
        /// </param>
        /// <returns>
        /// Returns boolean Value
        /// </returns>
        public static bool IsNumeric(object valueToCheck)
        {
            double dummy;
            string inputValue = Convert.ToString(valueToCheck);

            bool numeric = double.TryParse(inputValue, NumberStyles.Any, null, out dummy);

            return numeric;
        }

        /// <summary>
        /// Validates the <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The <paramref name="path"/>.
        /// </param>
        /// <returns>
        /// The validate <paramref name="path"/>.
        /// </returns>
        public static bool ValidatePath(string path)
        {
            return PathValidator.IsMatch(path);
        }

        /// <summary>
        /// Validates the name of the file.
        /// </summary>
        /// <param name="fileName">
        /// Name of the file.
        /// </param>
        /// <returns>
        /// The validate file name.
        /// </returns>
        public static bool ValidateFileName(string fileName)
        {
            return FileNameValidator.IsMatch(fileName);
        }

        /// <summary>
        /// Cleans the <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The <paramref name="path"/>.
        /// </param>
        /// <returns>
        /// The clean <paramref name="path"/>.
        /// </returns>
        public static string CleanPath(string path)
        {
            return PathCleaner.Replace(path, string.Empty);
        }

        /// <summary>
        /// Cleans the name of the file.
        /// </summary>
        /// <param name="fileName">
        /// Name of the file.
        /// </param>
        /// <returns>
        /// The clean file name.
        /// </returns>
        public static string CleanFileName(string fileName)
        {
            return FileNameCleaner.Replace(fileName, string.Empty);
        }

        /// <summary>
        /// Converts the Unicode chars to its to its ASCII equivalent.
        /// </summary>
        /// <param name="input">The <paramref name="input"/>.</param>
        /// <returns>The ASCII equivalent output</returns>
        public static string ConvertUnicodeChars(string input)
        {
            Regex regA = new Regex("[ã|à|â|ä|á|å]");
            Regex regAA = new Regex("[Ã|À|Â|Ä|Á|Å]");
            Regex regE = new Regex("[é|è|ê|ë]");
            Regex regEE = new Regex("[É|È|Ê|Ë]");
            Regex regI = new Regex("[í|ì|î|ï]");
            Regex regII = new Regex("[Í|Ì|Î|Ï]");
            Regex regO = new Regex("[õ|ò|ó|ô|ö]");
            Regex regOO = new Regex("[Õ|Ó|Ò|Ô|Ö]");
            Regex regU = new Regex("[ù|ú|û|ü|µ]");
            Regex regUU = new Regex("[Ü|Ú|Ù|Û]");
            Regex regY = new Regex("[ý|ÿ]");
            Regex regYY = new Regex("[Ý]");
            Regex regAE = new Regex("[æ]");
            Regex regAEAE = new Regex("[Æ]");
            Regex regOE = new Regex("[œ]");
            Regex regOEOE = new Regex("[Œ]");
            Regex regC = new Regex("[ç]");
            Regex regCC = new Regex("[Ç]");
            Regex regDD = new Regex("[Ð]");
            Regex regN = new Regex("[ñ]");
            Regex regNN = new Regex("[Ñ]");
            Regex regS = new Regex("[š]");
            Regex regSS = new Regex("[Š]");
            input = regA.Replace(input, "a");
            input = regAA.Replace(input, "A");
            input = regE.Replace(input, "e");
            input = regEE.Replace(input, "E");
            input = regI.Replace(input, "i");
            input = regII.Replace(input, "I");
            input = regO.Replace(input, "o");
            input = regOO.Replace(input, "O");
            input = regU.Replace(input, "u");
            input = regUU.Replace(input, "U");
            input = regY.Replace(input, "y");
            input = regYY.Replace(input, "Y");
            input = regAE.Replace(input, "ae");
            input = regAEAE.Replace(input, "AE");
            input = regOE.Replace(input, "oe");
            input = regOEOE.Replace(input, "OE");
            input = regC.Replace(input, "c");
            input = regCC.Replace(input, "C");
            input = regDD.Replace(input, "D");
            input = regN.Replace(input, "n");
            input = regNN.Replace(input, "N");
            input = regS.Replace(input, "s");
            input = regSS.Replace(input, "S");

            input = input.Replace("�", string.Empty);

            input = Encoding.ASCII.GetString(Encoding.GetEncoding(1251).GetBytes(input));

            input = input.Replace("?", string.Empty); //replace the unknown char which created in above.
            input = input.Replace("�", string.Empty);
            input = input.Replace("\t", string.Empty);
            input = input.Replace("@", "at");
            input = input.Replace("\r", string.Empty);
            input = input.Replace("\n", string.Empty);
            input = input.Replace("+", "_");

            return input;
        }

        /// <summary>
        /// Checks if user has write access to the folder.
        /// </summary>
        /// <param name="folderId">The folder id.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns>
        /// Returns if the user has write access to the folder.
        /// </returns>
        public static bool CheckIfUserHasFolderWriteAccess(int folderId, PortalSettings portalSettings)
        {
            try
            {
                var checkFolder = folderId.Equals(-1)
                                      ? ConvertFilePathToFolderInfo(portalSettings.HomeDirectoryMapPath, portalSettings)
                                      : FolderManager.Instance.GetFolder(folderId);

                return FolderPermissionController.HasFolderPermission(checkFolder.FolderPermissions, "WRITE");
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Converts a File Path to a Folder Info
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns>
        /// Returns the Folder Info
        /// </returns>
        public static IFolderInfo ConvertFilePathToFolderInfo(string folderPath, PortalSettings portalSettings)
        {
            try
            {
                folderPath = folderPath.Substring(portalSettings.HomeDirectoryMapPath.Length).Replace("\\", "/");
            }
            catch (Exception)
            {
                folderPath = folderPath.Replace("\\", "/");
            }

            return FolderManager.Instance.GetFolder(portalSettings.PortalId, folderPath);
        }

        /// <summary>
        /// Converts a File Path to a file Info
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <returns>
        /// Returns the file Info
        /// </returns>
        public static int ConvertFilePathToFileId(string filePath, int portalID)
        {
            var fileName = Path.GetFileName(filePath);

            if (fileName == null)
            {
                return Null.NullInteger;
            }

            var folderPath = filePath.Substring(0, filePath.LastIndexOf(fileName, StringComparison.Ordinal));
            var folder = FolderManager.Instance.GetFolder(portalID, folderPath);

            var file = FileManager.Instance.GetFile(folder, fileName);

            return file.FileId;
        }

        /// <summary>
        /// Sort a List Ascending
        /// </summary>
        /// <param name="source">
        /// The <paramref name="source"/> List
        /// </param>
        /// <param name="selector">
        /// The selector.
        /// </param>
        /// <typeparam name="TSource">
        /// The TSource List Item
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The TValue List item
        /// </typeparam>
        public static void SortAscending<TSource, TValue>(List<TSource> source, Func<TSource, TValue> selector)
        {
            var comparer = Comparer<TValue>.Default;
            source.Sort((x, y) => comparer.Compare(selector(x), selector(y)));
        }

        /// <summary>
        /// Sort a List Descending
        /// </summary>
        /// <param name="source">
        /// The <paramref name="source"/> List
        /// </param>
        /// <param name="selector">
        /// The selector.
        /// </param>
        /// <typeparam name="TSource">
        /// The TSource List Item
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The TValue List item
        /// </typeparam>
        public static void SortDescending<TSource, TValue>(List<TSource> source, Func<TSource, TValue> selector)
        {
            var comparer = Comparer<TValue>.Default;
            source.Sort((x, y) => comparer.Compare(selector(y), selector(x)));
        }

        /// <summary>
        /// Determines whether user is in the specified role.
        /// </summary>
        /// <param name="roles">The roles.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>
        ///   <c>true</c> if [is in roles] [the specified roles]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInRoles(string roles, PortalSettings settings)
        {
            var objUserInfo = UserController.Instance.GetCurrentUserInfo();

            var isInRoles = objUserInfo.IsSuperUser;

            if (isInRoles)
            {
                // super user
                return true;
            }

            var context = HttpContext.Current;

            foreach (var role in roles.Split(';').Where(role => !string.IsNullOrEmpty(role)))
            {
                if (role.StartsWith("!"))
                {
                    var denyRole = role.Replace("!", string.Empty);

                    if ((!context.Request.IsAuthenticated && denyRole.Equals(Globals.glbRoleUnauthUserName)) || denyRole.Equals(Globals.glbRoleAllUsersName) ||
                        objUserInfo.IsInRole(denyRole))
                    {
                        break;
                    }
                }
                else
                {
                    if (settings.PortalId == objUserInfo.PortalID
                        && settings.AdministratorId == objUserInfo.UserID)
                    {
                        isInRoles = true;
                        break;
                    }

                    if ((context.Request.IsAuthenticated || !role.Equals(Globals.glbRoleUnauthUserName))
                        && !role.Equals(Globals.glbRoleAllUsersName) && !objUserInfo.IsInRole(role))
                    {
                        continue;
                    }

                    isInRoles = true;
                    break;
                }
            }

            return isInRoles;
        }

        /// <summary>
        /// Finds the control.
        /// </summary>
        /// <typeparam name="T">The Control Type</typeparam>
        /// <param name="startingControl">The starting control.</param>
        /// <param name="id">The <paramref name="id"/>.</param>
        /// <returns>Returns the Control</returns>
        public static T FindControl<T>(Control startingControl, string id) where T : Control
        {
            // this is null by default
            T found = default(T);

            int controlCount = startingControl.Controls.Count;

            if (controlCount > 0)
            {
                for (int i = 0; i < controlCount; i++)
                {
                    Control activeControl = startingControl.Controls[i];

                    if (activeControl is T)
                    {
                        found = startingControl.Controls[i] as T;

                        if (found.ID.ToLower().Contains(id.ToLower()))
                        {
                            break;
                        }

                        found = null;
                    }
                    else
                    {
                        found = FindControl<T>(activeControl, id);

                        if (found != null)
                        {
                            break;
                        }
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// Gets the size of the max upload.
        /// </summary>
        /// <param name="inkilobytes">if set to <c>true</c> Returns Value as kilo byte otherwise as byte.</param>
        /// <returns>
        /// Returns the Max. Upload Size
        /// </returns>
        public static long GetMaxUploadSize(bool inkilobytes = false)
        {
            long result;

            try
            {
                var section =
                    HttpContext.Current.GetSection("system.web/httpRuntime") as
                    HttpRuntimeSection;
                result = section.MaxRequestLength;
            }
            catch (Exception)
            {
                result = 0;
            }

            return inkilobytes ? result : (result * 1024);
        }
    }
}