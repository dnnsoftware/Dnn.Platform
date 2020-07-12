// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FriendlyUrlControllerTests
    {
        private Mock<CachingProvider> _mockCache;

        [SetUp]
        public void SetUp()
        {
            this._mockCache = MockComponentProvider.CreateNew<CachingProvider>();
        }

        [Test]
        public void DoesNothingToSimpleText()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            Assert.IsFalse(replacedUnwantedChars);
            Assert.AreEqual("abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", result);
        }

        [Test]
        public void RemoveSpace()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("123 abc", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            Assert.IsFalse(replacedUnwantedChars);
            Assert.AreEqual("123abc", result);
        }

        [Test]
        public void ReplaceSpaceWithHyphen()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("123 abc", CreateFriendlyUrlOptions(replaceSpaceWith: "-"), out replacedUnwantedChars);

            Assert.IsFalse(replacedUnwantedChars);
            Assert.AreEqual("123-abc", result);
        }

        [Test]
        public void RemoveApostrophe()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("Fred's House", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            Assert.IsTrue(replacedUnwantedChars);
            Assert.AreEqual("FredsHouse", result);
        }

        [Test]
        public void RemoveCharactersInReplaceList()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl(@"a b&c$d+e,f/g?h~i#j<k>l(m)n¿o¡p«q»r!s""t", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            Assert.IsTrue(replacedUnwantedChars);
            Assert.AreEqual("abcdefghijklmnopqrst", result);
        }

        [Test]
        public void ReplaceCharactersInReplaceList()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl(@"a b&c$d+e,f/g?h~i#j<k>l(m)n¿o¡p«q»r!s""t", CreateFriendlyUrlOptions(replaceSpaceWith: "_"), out replacedUnwantedChars);

            Assert.IsTrue(replacedUnwantedChars);
            Assert.AreEqual("a_b_c_d_e_f_g_h_i_j_k_l_m_n_o_p_q_r_s_t", result);
        }

        [Test]
        public void RemoveCharactersInReplaceListWhenReplacementCharacterIsNotAMatchingCharacter()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl(@"a b&c$d+e,f/g?h~i#j<k>l(m)n¿o¡p«q»r!s""t", CreateFriendlyUrlOptions(replaceSpaceWith: "."), out replacedUnwantedChars);

            Assert.IsTrue(replacedUnwantedChars);
            Assert.AreEqual("abcdefghijklmnopqrst", result);
        }

        [Test]
        public void RemovePunctuatuation()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("Dr. Cousteau, where are you?", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            Assert.IsTrue(replacedUnwantedChars);
            Assert.AreEqual("DrCousteauwhereareyou", result);
        }

        [Test]
        public void RemoveDoubleReplacements()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("This, .Has Lots Of---Replacements   Don't you think?", CreateFriendlyUrlOptions(replaceSpaceWith: "-"), out replacedUnwantedChars);

            Assert.IsTrue(replacedUnwantedChars);
            Assert.AreEqual("This-Has-Lots-Of-Replacements-Dont-you-think", result);
        }

        [Test]
        public void DoNotRemoveDoubleReplacements()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("This, ,Has Lots Of---Replacements   Don't you think?", CreateFriendlyUrlOptions(replaceDoubleChars: false, replaceSpaceWith: "-"), out replacedUnwantedChars);

            Assert.IsTrue(replacedUnwantedChars);
            Assert.AreEqual("This---Has-Lots-Of---Replacements---Dont-you-think", result);
        }

        [Test]
        public void DoNotConvertVietnameseDiacritics()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("DấuNgãSắcHuyềnNặngHỏi", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            Assert.IsFalse(replacedUnwantedChars);
            Assert.AreEqual("DấuNgãSắcHuyềnNặngHỏi", result);
        }

        [Test]
        public void DoNotConvertFrenchDiacritics()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("CrèmeFraîcheCédille", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            Assert.IsFalse(replacedUnwantedChars);
            Assert.AreEqual("CrèmeFraîcheCédille", result);
        }

        [Test]
        public void DoNotConvertRussianDiacritics()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("писа́тьбо́льшая", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            Assert.IsFalse(replacedUnwantedChars);
            Assert.AreEqual("писа́тьбо́льшая", result);
        }

        [Test]
        public void DoNotConvertLeoneseDiacritics()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("ñavidá", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            Assert.IsFalse(replacedUnwantedChars);
            Assert.AreEqual("ñavidá", result);
        }

        [Test]
        public void ConvertVietnameseDiacritics()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("DấuNgãSắcHuyềnNặngHỏi", CreateFriendlyUrlOptions(autoAsciiConvert: true), out replacedUnwantedChars);

            Assert.IsTrue(replacedUnwantedChars);
            Assert.AreEqual("DauNgaSacHuyenNangHoi", result);
        }

        [Test]
        public void ConvertFrenchDiacritics()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("CrèmeFraîcheCédille", CreateFriendlyUrlOptions(autoAsciiConvert: true), out replacedUnwantedChars);

            Assert.IsTrue(replacedUnwantedChars);
            Assert.AreEqual("CremeFraicheCedille", result);
        }

        [Test]
        public void ConvertRussianDiacritics()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("писа́тьбо́льшая", CreateFriendlyUrlOptions(autoAsciiConvert: true), out replacedUnwantedChars);

            Assert.IsTrue(replacedUnwantedChars);
            Assert.AreEqual("писатьбольшая", result);
        }

        [Test]
        public void ConvertLeoneseDiacritics()
        {
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("ñavidá", CreateFriendlyUrlOptions(autoAsciiConvert: true), out replacedUnwantedChars);

            Assert.IsTrue(replacedUnwantedChars);
            Assert.AreEqual("navida", result);
        }

        [Test]
        public void ReplaceBeforeConvertingDiacritics()
        {
            bool replacedUnwantedChars;
            var replacements = new Dictionary<string, string>(1) { { "ñ", "nn" }, };
            string result = FriendlyUrlController.CleanNameForUrl("Carreño", CreateFriendlyUrlOptions(replaceCharacterDictionary: replacements), out replacedUnwantedChars);

            Assert.IsTrue(replacedUnwantedChars);
            Assert.AreEqual("Carrenno", result);
        }

        private static FriendlyUrlOptions CreateFriendlyUrlOptions(
            string replaceSpaceWith = FriendlyUrlSettings.ReplaceSpaceWithNothing,
            string spaceEncodingValue = FriendlyUrlSettings.SpaceEncodingHex,
            bool autoAsciiConvert = false,
            string regexMatch = @"[^\w\d _-]",
            string illegalChars = @"<>/\?:&=+|%#",
            string replaceChars = @" &$+,/?~#<>()¿¡«»!""",
            bool replaceDoubleChars = true,
            Dictionary<string, string> replaceCharacterDictionary = null,
            PageExtensionUsageType pageExtensionUsageType = PageExtensionUsageType.Never,
            string pageExtension = ".aspx")
        {
            replaceCharacterDictionary = replaceCharacterDictionary ?? new Dictionary<string, string>(0);
            return new FriendlyUrlOptions
            {
                PunctuationReplacement = (replaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing)
                                                ? replaceSpaceWith
                                                : string.Empty,
                SpaceEncoding = spaceEncodingValue,
                MaxUrlPathLength = 200,
                ConvertDiacriticChars = autoAsciiConvert,
                RegexMatch = regexMatch,
                IllegalChars = illegalChars,
                ReplaceChars = replaceChars,
                ReplaceDoubleChars = replaceDoubleChars,
                ReplaceCharWithChar = replaceCharacterDictionary,
                PageExtension = (pageExtensionUsageType == PageExtensionUsageType.Never)
                                        ? string.Empty
                                        : pageExtension,
            };
        }
    }
}
