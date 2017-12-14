#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Web.Caching;

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Providers.Caching;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;

using Moq;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Providers.Caching
{
    /// <summary>
    ///   Summary description for DataCacheTests
    /// </summary>
    [TestFixture]
    public class DataCacheTests
    {
        private Mock<CachingProvider> mockCache;

        #region Test Initialization and Cleanup

        [SetUp]
        public void SetUp()
        {
            //Create a Container
            ComponentFactory.Container = new SimpleContainer();

            mockCache = MockComponentProvider.CreateNew<CachingProvider>();
        }

        #endregion

        #region GetCache Tests

        [Test]
        public void DataCache_GetCache_Should_Return_On_Correct_CacheKey()
        {
            // Arrange
            mockCache.Setup(cache => cache.GetItem(GetDnnCacheKey(Constants.CACHEING_ValidKey))).ReturnsValidValue();

            // Act
            object cacheValue = DataCache.GetCache(Constants.CACHEING_ValidKey);

            // Assert
            Assert.IsInstanceOf<string>(cacheValue);
            Assert.AreEqual(Constants.CACHEING_ValidValue, cacheValue);
        }

        [Test]
        public void DataCache_GetCache_Should_Return_Null_On_Incorrect_CacheKey()
        {
            // Arrange
            mockCache.Setup(cache => cache.GetItem(GetDnnCacheKey(Constants.CACHEING_InValidKey))).Returns(null);

            // Act
            object cacheValue = DataCache.GetCache(Constants.CACHEING_InValidKey);

            // Assert
            Assert.IsNull(cacheValue);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_GetCache_Should_Throw_On_NullOrEmpty_CacheKey(string key)
        {
            Assert.Throws<ArgumentException>(() => DataCache.GetCache(key));
        }

        #endregion

        #region GetCache Tests

        [Test]
        public void DataCache_GetCacheOfT_Should_Return_On_Correct_CacheKey()
        {
            // Arrange
            mockCache.Setup(cache => cache.GetItem(GetDnnCacheKey(Constants.CACHEING_ValidKey))).ReturnsValidValue();

            // Act
            object cacheValue = DataCache.GetCache<string>(Constants.CACHEING_ValidKey);

            // Assert
            Assert.IsInstanceOf<string>(cacheValue);
            Assert.AreEqual(Constants.CACHEING_ValidValue, cacheValue);
        }

        [Test]
        public void DataCache_GetCacheOfT_Should_Return_Null_On_Incorrect_CacheKey()
        {
            // Arrange
            mockCache.Setup(cache => cache.GetItem(GetDnnCacheKey(Constants.CACHEING_InValidKey))).Returns(null);

            // Act
            object cacheValue = DataCache.GetCache<string>(Constants.CACHEING_InValidKey);

            // Assert
            Assert.IsNull(cacheValue);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_GetCacheOfT_Should_Throw_On_NullOrEmpty_CacheKey(string key)
        {
            Assert.Throws<ArgumentException>(() => DataCache.GetCache<string>(key));
        }

        #endregion

        #region RemoveCache Tests

        [Test]
        public void DataCache_RemoveCache_Should_Succeed_On_Valid_CacheKey()
        {
            // Arrange

            // Act
            DataCache.RemoveCache(Constants.CACHEING_ValidKey);

            // Assert
            mockCache.Verify(cache => cache.Remove(GetDnnCacheKey(Constants.CACHEING_ValidKey)));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_RemoveCache_Should_Throw_On_NullOrEmpty_CacheKey(string key)
        {
            Assert.Throws<ArgumentException>(() => DataCache.RemoveCache(key));
        }

        #endregion

        #region SetCache Tests

        #region SetCache(string, object) Tests

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_Should_Throw_On_Null_CacheKey(string key)
        {
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue));
        }

        [Test]
        public void DataCache_SetCache_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
            // Arrange

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue);

            // Assert
            DNNCacheDependency dep = null;
            mockCache.Verify(
                cache =>
                cache.Insert(GetDnnCacheKey(Constants.CACHEING_ValidKey),
                             Constants.CACHEING_ValidValue,
                             dep,
                             Cache.NoAbsoluteExpiration,
                             Cache.NoSlidingExpiration,
                             CacheItemPriority.Normal,
                             DataCache.ItemRemovedCallback));
        }

        #endregion

        #region SetCache(string, object, CacheDependency) Tests

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_With_Dependency_Should_Throw_On_Null_CacheKey(string key)
        {
            DNNCacheDependency dep = CreateTestDependency(); // Dependency type or value doesn't matter
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue, dep));
        }

        [Test]
        public void DataCache_SetCache_With_Dependency_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
            // Arrange
            DNNCacheDependency dep = CreateTestDependency(); // Dependency type or value doesn't matter

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, dep);

            // Assert
            mockCache.Verify(
                cache =>
                cache.Insert(GetDnnCacheKey(Constants.CACHEING_ValidKey),
                             Constants.CACHEING_ValidValue,
                             dep,
                             Cache.NoAbsoluteExpiration,
                             Cache.NoSlidingExpiration,
                             CacheItemPriority.Normal,
                             DataCache.ItemRemovedCallback));
        }

        #endregion

        #region SetCache(string, object, DateTime) Tests

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_With_AbsoluteExpiration_Should_Throw_On_Null_CacheKey(string key)
        {
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue, absExpiry));
        }

        [Test]
        public void DataCache_SetCache_With_AbsoluteExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
            // Arrange
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, absExpiry);

            // Assert
            DNNCacheDependency dep = null;
            mockCache.Verify(
                cache =>
                cache.Insert(GetDnnCacheKey(Constants.CACHEING_ValidKey),
                             Constants.CACHEING_ValidValue,
                             dep,
                             absExpiry,
                             Cache.NoSlidingExpiration,
                             CacheItemPriority.Normal,
                             DataCache.ItemRemovedCallback));
        }

        #endregion

        #region SetCache(string, object, TimeSpan) Tests

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_With_SlidingExpiration_Should_Throw_On_Null_CacheKey(string key)
        {
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue, slidingExpiry));
        }

        [Test]
        public void DataCache_SetCache_With_SlidingExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
            // Arrange
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, slidingExpiry);

            // Assert
            // Assert
            DNNCacheDependency dep = null;
            mockCache.Verify(
                cache =>
                cache.Insert(GetDnnCacheKey(Constants.CACHEING_ValidKey),
                             Constants.CACHEING_ValidValue,
                             dep,
                             Cache.NoAbsoluteExpiration,
                             slidingExpiry,
                             CacheItemPriority.Normal,
                             DataCache.ItemRemovedCallback));
        }

        #endregion

        #region SetCache(string, object, CacheDependency, DateTime, TimeSpan) Tests

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Throw_On_Null_CacheKey(string key)
        {
            DNNCacheDependency dep = CreateTestDependency(); // Dependency type or value doesn't matter
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry));
        }

        [Test]
        public void DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
            // Arrange
            DNNCacheDependency dep = CreateTestDependency(); // Dependency type or value doesn't matter
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry);

            // Assert
            mockCache.Verify(
                cache =>
                cache.Insert(GetDnnCacheKey(Constants.CACHEING_ValidKey), Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, CacheItemPriority.Normal, DataCache.ItemRemovedCallback));
        }

        #endregion

        #region SetCache(string, object, CacheDependency, DateTime, TimeSpan, CacheItemPriority, CacheItemRemovedCallback) Tests

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_With_Priority_Should_Throw_On_Null_CacheKey(string key)
        {
            DNNCacheDependency dep = CreateTestDependency(); // Dependency type or value doesn't matter
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
            CacheItemPriority priority = CacheItemPriority.High; // Priority doesn't matter
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, priority, null));
        }

        [Test]
        public void DataCache_SetCache_With_Priority_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
            // Arrange
            DNNCacheDependency dep = CreateTestDependency(); // Dependency type or value doesn't matter
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
            CacheItemPriority priority = CacheItemPriority.High; // Priority doesn't matter

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, priority, null);

            // Assert
            mockCache.Verify(cache => cache.Insert(GetDnnCacheKey(Constants.CACHEING_ValidKey), Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, priority, DataCache.ItemRemovedCallback));
        }

        [Test]
        public void DataCache_SetCache_With_Callback_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
            // Arrange
            DNNCacheDependency dep = CreateTestDependency(); // Dependency type or value doesn't matter
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
            CacheItemRemovedCallback callback = ItemRemovedCallback;

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, CacheItemPriority.Normal, ItemRemovedCallback);

            // Assert
            mockCache.Verify(
                cache => cache.Insert(GetDnnCacheKey(Constants.CACHEING_ValidKey), Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, CacheItemPriority.Normal, ItemRemovedCallback));
        }

        #endregion

        #endregion

        #region Private Helper Methods

        private DNNCacheDependency CreateTestDependency()
        {
            return new DNNCacheDependency("C:\\testdependency.txt");
        }

        private string GetDnnCacheKey(string cacheKey)
        {
            return String.Format("DNN_{0}", cacheKey);
        }

        private void ItemRemovedCallback(string key, object value, CacheItemRemovedReason removedReason)
        {
            //do nothing
        }

        #endregion
    }
}