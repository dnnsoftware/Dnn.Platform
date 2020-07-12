// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users.Membership
{
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

    public class MembershipPasswordController
    {
        private readonly DataProvider _dataProvider = DataProvider.Instance();

        /// <summary>
        /// returns the password history of the supplied user.
        /// </summary>
        /// <returns>list of PasswordHistory objects.</returns>
        public List<PasswordHistory> GetPasswordHistory(int userId)
        {
            return this.GetPasswordHistory(userId, Null.NullInteger);
        }

        /// <summary>
        /// returns the password history of the supplied user.
        /// </summary>
        /// <param name="portalId">portalid - futureproofing against any setting become site level.</param>
        /// <returns>list of PasswordHistory objects.</returns>
        public List<PasswordHistory> GetPasswordHistory(int userId, int portalId)
        {
            var settings = new MembershipPasswordSettings(portalId);
            List<PasswordHistory> history =
                CBO.FillCollection<PasswordHistory>(this._dataProvider.GetPasswordHistory(userId, settings.NumberOfPasswordsStored, settings.NumberOfDaysBeforePasswordReuse));
            return history;
        }

        /// <summary>
        /// checks to see if the password is in history and adds it if it is not.
        /// </summary>
        /// <param name="portalId">portalid - futureproofing against any setting become site level.</param>
        /// <param name="newPassword">users new password suggestion.</param>
        /// <returns>true if password has not been used in users history, false otherwise.</returns>
        public bool IsPasswordInHistory(int userId, int portalId, string newPassword)
        {
            return this.IsPasswordInHistory(userId, portalId, newPassword, true);
        }

        /// <summary>
        /// checks to see if the password is in history and adds it if it is not.
        /// </summary>
        /// <param name="portalId">portalid - futureproofing against any setting become site level.</param>
        /// <param name="newPassword">users new password suggestion.</param>
        /// <param name="autoAdd">If set true then add the password into history if its not used yet.</param>
        /// <returns>true if password has not been used in users history, false otherwise.</returns>
        public bool IsPasswordInHistory(int userId, int portalId, string newPassword, bool autoAdd)
        {
            Requires.NotNullOrEmpty("newPassword", newPassword);
            bool isPreviouslyUsed = false;
            var settings = new MembershipPasswordSettings(portalId);
            if (settings.EnablePasswordHistory)
            {
                if (!this.IsPasswordPreviouslyUsed(userId, newPassword))
                {
                    if (autoAdd)
                    {
                        this.AddPasswordHistory(userId, newPassword, settings.NumberOfPasswordsStored, settings.NumberOfDaysBeforePasswordReuse);
                    }
                }
                else
                {
                    isPreviouslyUsed = true;
                }
            }

            return isPreviouslyUsed;
        }

        /// <summary>
        /// checks if the new password matches a previously used password when hashed with the same salt.
        /// </summary>
        /// <param name="password">users entered new password.</param>
        /// <returns>true if previously used, false otherwise.</returns>
        public bool IsPasswordPreviouslyUsed(int userId, string password)
        {
            bool foundMatch = false;

            // use default algorithm (SHA1CryptoServiceProvider )
            using (HashAlgorithm ha = HashAlgorithm.Create())
            {
                List<PasswordHistory> history = this.GetPasswordHistory(userId);
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
                    {
                        foundMatch = true;
                    }
                }
            }

            return foundMatch;
        }

        /// <summary>
        /// checks if the password reset token being used is valid i.e. has not been used before and is within the the expiration period.
        /// </summary>
        /// <param name="userId">user attempting to reset their password.</param>
        /// <param name="resetToken">reset token supplied via email link.</param>
        /// <returns>true if value matches (so has not been used before) and is within expiration window.</returns>
        public bool IsValidToken(int userId, Guid resetToken)
        {
            if (UserController.Instance.GetCurrentUserInfo().PasswordResetToken == resetToken &&
                UserController.Instance.GetCurrentUserInfo().PasswordResetExpiration <= DateTime.Now)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if user entered password is on the list of banned passwords
        /// combines host level list with current site level list.
        /// </summary>
        /// <param name="inputString">user entered password.</param>
        /// <returns>true if password found, false otherwise.</returns>
        public bool FoundBannedPassword(string inputString)
        {
            const string listName = "BannedPasswords";

            var listController = new ListController();
            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();

            IEnumerable<ListEntryInfo> listEntryHostInfos = listController.GetListEntryInfoItems(listName, string.Empty,
                Null.NullInteger);
            IEnumerable<ListEntryInfo> listEntryPortalInfos =
                listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, string.Empty, settings.PortalId);

            IEnumerable<ListEntryInfo> query2 = listEntryHostInfos.Where(test => test.Text == inputString);
            IEnumerable<ListEntryInfo> query3 = listEntryPortalInfos.Where(test => test.Text == inputString);

            if (query2.Any() || query3.Any())
            {
                return true;
            }

            return false;
        }

        private void AddPasswordHistory(int userId, string password, int passwordsRetained, int daysRetained)
        {
            using (HashAlgorithm ha = HashAlgorithm.Create())
            {
                byte[] newSalt = this.GetRandomSaltValue();
                byte[] bytePassword = Encoding.Unicode.GetBytes(password);
                var inputBuffer = new byte[bytePassword.Length + 16];
                Buffer.BlockCopy(bytePassword, 0, inputBuffer, 0, bytePassword.Length);
                Buffer.BlockCopy(newSalt, 0, inputBuffer, bytePassword.Length, 16);
                byte[] bhashedPassword = ha.ComputeHash(inputBuffer);
                string hashedPassword = Convert.ToBase64String(bhashedPassword);

                this._dataProvider.AddPasswordHistory(userId, hashedPassword, Convert.ToBase64String(newSalt), passwordsRetained, daysRetained);
            }
        }

        private byte[] GetRandomSaltValue()
        {
            using (var rcsp = new RNGCryptoServiceProvider())
            {
                var bSalt = new byte[16];
                rcsp.GetBytes(bSalt);
                return bSalt;
            }
        }
    }
}
