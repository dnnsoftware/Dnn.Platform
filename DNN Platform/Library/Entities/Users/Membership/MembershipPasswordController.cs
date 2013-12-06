#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Entities.Users.Membership
{
    public class MembershipPasswordController
    {
        #region Private Members

        private readonly DataProvider _dataProvider = DataProvider.Instance();

        #endregion

        #region Private functions

        private void AddPasswordHistory(int userId, string password, int retained)
        {
            HashAlgorithm ha = HashAlgorithm.Create();
            byte[] newSalt = GetRandomSaltValue();
            byte[] bytePassword = Encoding.Unicode.GetBytes(password);
            var inputBuffer = new byte[bytePassword.Length + 16];
            Buffer.BlockCopy(bytePassword, 0, inputBuffer, 0, bytePassword.Length);
            Buffer.BlockCopy(newSalt, 0, inputBuffer, bytePassword.Length, 16);
            byte[] bhashedPassword = ha.ComputeHash(inputBuffer);
            string hashedPassword = Convert.ToBase64String(bhashedPassword);

            _dataProvider.AddPasswordHistory(userId, hashedPassword,
                Convert.ToBase64String(newSalt), retained);
        }

        private byte[] GetRandomSaltValue()
        {
            var rcsp = new RNGCryptoServiceProvider();
            var bSalt = new byte[16];
            rcsp.GetBytes(bSalt);
            return bSalt;
        }

        #endregion

        /// <summary>
        /// returns the password history of the supplied user
        /// </summary>
        /// <returns>list of PasswordHistory objects</returns>
        public List<PasswordHistory> GetPasswordHistory(int userId)
        {
            List<PasswordHistory> history =
                CBO.FillCollection<PasswordHistory>(_dataProvider.GetPasswordHistory(userId));
            return history;
        }

        /// <summary>
        /// checks to see if the password is in history and adds it if it is not
        /// </summary>
        /// <param name="portalId">portalid - futureproofing against any setting become site level</param>
        /// <param name="newPassword">users new password suggestion</param>
        /// <returns>true if password has not been used in users history, false otherwise</returns>
        public bool IsPasswordInHistory(int userId, int portalId, string newPassword)
        {
            Requires.NotNullOrEmpty("newPassword", newPassword);
            bool isPreviouslyUsed = false;
            var settings = new MembershipPasswordSettings(portalId);
            if (settings.EnablePasswordHistory)
            {
                if (IsPasswordPreviouslyUsed(userId, newPassword) == false)
                {
                    AddPasswordHistory(userId, newPassword, settings.NumberOfPasswordsStored);
                }
                else
                {
                    isPreviouslyUsed = true;
                }
            }
            return isPreviouslyUsed;
        }

        /// <summary>
        /// checks if the new password matches a previously used password when hashed with the same salt
        /// </summary>
        /// <param name="password">users entered new password</param>
        /// <returns>true if previously used, false otherwise</returns>
        public bool IsPasswordPreviouslyUsed(int userId, string password)
        {
            //use default algorithm (SHA1CryptoServiceProvider )
            HashAlgorithm ha = HashAlgorithm.Create();
            bool foundMatch = false;

            List<PasswordHistory> history = GetPasswordHistory(userId);
            foreach (PasswordHistory ph in history)
            {
                string oldEncodedPassword = ph.Password;
                string oldEncodedSalt = ph.PasswordSalt;
                byte[] oldSalt = Convert.FromBase64String(oldEncodedSalt);
                byte[] bytePassword = Encoding.Unicode.GetBytes(password);
                var inputBuffer = new byte[bytePassword.Length + 16];
                Buffer.BlockCopy(bytePassword, 0, inputBuffer, 0, bytePassword.Length);
                Buffer.BlockCopy(oldSalt, 0, inputBuffer, bytePassword.Length, 16);
                byte[] bhashedPassword = ha.ComputeHash(inputBuffer);
                string hashedPassword = Convert.ToBase64String(bhashedPassword);
                if (hashedPassword == oldEncodedPassword)
                    foundMatch = true;
            }

            return foundMatch;
        }

        /// <summary>
        /// checks if the password reset token being used is valid i.e. has not been used before and is within the the expiration period
        /// </summary>
        /// <param name="userId">user attempting to reset their password</param>
        /// <param name="resetToken">reset token supplied via email link</param>
        /// <returns>true if value matches (so has not been used before) and is within expiration window</returns>
        public bool IsValidToken(int userId, Guid resetToken)
        {
            if (UserController.GetCurrentUserInfo().PasswordResetToken == resetToken &&
                UserController.GetCurrentUserInfo().PasswordResetExpiration <= DateTime.Now)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if user entered password is on the list of banned passwords
        /// combines host level list with current site level list
        /// </summary>
        /// <param name="inputString">user entered password</param>
        /// <returns>true if password found, false otherwise</returns>
        public bool FoundBannedPassword(string inputString)
        {
            const string listName = "BannedPasswords";

            var listController = new ListController();
            PortalSettings settings = PortalController.GetCurrentPortalSettings();

            IEnumerable<ListEntryInfo> listEntryHostInfos = listController.GetListEntryInfoItems(listName, "",
                Null.NullInteger);
            IEnumerable<ListEntryInfo> listEntryPortalInfos =
                listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, "", settings.PortalId);

            IEnumerable<ListEntryInfo> query2 = listEntryHostInfos.Where(test => test.Text == inputString);
            IEnumerable<ListEntryInfo> query3 = listEntryPortalInfos.Where(test => test.Text == inputString);

            if (query2.Any() || query3.Any())
            {
                return true;
            }

            return false;
        }
    }
}