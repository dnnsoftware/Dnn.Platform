// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Caching
{
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

    /// <summary>
    ///   Summary description for DataCacheTests.
    /// </summary>
    [TestFixture]
    public class DataCacheTests
    {
        private Mock<CachingProvider> mockCache;

        [SetUp]
        public void SetUp()
        {
            // Create a Container
            ComponentFactory.Container = new SimpleContainer();

            this.mockCache = MockComponentProvider.CreateNew<CachingProvider>();
        }

        [Test]
        public void DataCache_GetCache_Should_Return_On_Correct_CacheKey()
        {
            // Arrange
            this.mockCache.Setup(cache => cache.GetItem(this.GetDnnCacheKey(Constants.CACHEING_ValidKey))).ReturnsValidValue();

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
            this.mockCache.Setup(cache => cache.GetItem(this.GetDnnCacheKey(Constants.CACHEING_InValidKey))).Returns(null);

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

        [Test]
        public void DataCache_GetCacheOfT_Should_Return_On_Correct_CacheKey()
        {
            // Arrange
            this.mockCache.Setup(cache => cache.GetItem(this.GetDnnCacheKey(Constants.CACHEING_ValidKey))).ReturnsValidValue();

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
            this.mockCache.Setup(cache => cache.GetItem(this.GetDnnCacheKey(Constants.CACHEING_InValidKey))).Returns(null);

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

        [Test]
        public void DataCache_RemoveCache_Should_Succeed_On_Valid_CacheKey()
        {
            // Arrange

            // Act
            DataCache.RemoveCache(Constants.CACHEING_ValidKey);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(this.GetDnnCacheKey(Constants.CACHEING_ValidKey)));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_RemoveCache_Should_Throw_On_NullOrEmpty_CacheKey(string key)
        {
            Assert.Throws<ArgumentException>(() => DataCache.RemoveCache(key));
        }

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
            this.mockCache.Verify(
                cache =>
                cache.Insert(
                    this.GetDnnCacheKey(Constants.CACHEING_ValidKey),
                    Constants.CACHEING_ValidValue,
                    dep,
                    Cache.NoAbsoluteExpiration,
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    DataCache.ItemRemovedCallback));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_With_Dependency_Should_Throw_On_Null_CacheKey(string key)
        {
            DNNCacheDependency dep = this.CreateTestDependency(); // Dependency type or value doesn't matter
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue, dep));
        }

        [Test]
        public void DataCache_SetCache_With_Dependency_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
            // Arrange
            DNNCacheDependency dep = this.CreateTestDependency(); // Dependency type or value doesn't matter

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, dep);

            // Assert
            this.mockCache.Verify(
                cache =>
                cache.Insert(
                    this.GetDnnCacheKey(Constants.CACHEING_ValidKey),
                    Constants.CACHEING_ValidValue,
                    dep,
                    Cache.NoAbsoluteExpiration,
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    DataCache.ItemRemovedCallback));
        }

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
            this.mockCache.Verify(
                cache =>
                cache.Insert(
                    this.GetDnnCacheKey(Constants.CACHEING_ValidKey),
                    Constants.CACHEING_ValidValue,
                    dep,
                    absExpiry,
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    DataCache.ItemRemovedCallback));
        }

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
            this.mockCache.Verify(
                cache =>
                cache.Insert(
                    this.GetDnnCacheKey(Constants.CACHEING_ValidKey),
                    Constants.CACHEING_ValidValue,
                    dep,
                    Cache.NoAbsoluteExpiration,
                    slidingExpiry,
                    CacheItemPriority.Normal,
                    DataCache.ItemRemovedCallback));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Throw_On_Null_CacheKey(string key)
        {
            DNNCacheDependency dep = this.CreateTestDependency(); // Dependency type or value doesn't matter
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry));
        }

        [Test]
        public void DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
            // Arrange
            DNNCacheDependency dep = this.CreateTestDependency(); // Dependency type or value doesn't matter
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry);

            // Assert
            this.mockCache.Verify(
                cache =>
                cache.Insert(this.GetDnnCacheKey(Constants.CACHEING_ValidKey), Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, CacheItemPriority.Normal, DataCache.ItemRemovedCallback));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_With_Priority_Should_Throw_On_Null_CacheKey(string key)
        {
            DNNCacheDependency dep = this.CreateTestDependency(); // Dependency type or value doesn't matter
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
            CacheItemPriority priority = CacheItemPriority.High; // Priority doesn't matter
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, priority, null));
        }

        [Test]
        public void DataCache_SetCache_With_Priority_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
            // Arrange
            DNNCacheDependency dep = this.CreateTestDependency(); // Dependency type or value doesn't matter
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
            CacheItemPriority priority = CacheItemPriority.High; // Priority doesn't matter

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, priority, null);

            // Assert
            this.mockCache.Verify(cache => cache.Insert(this.GetDnnCacheKey(Constants.CACHEING_ValidKey), Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, priority, DataCache.ItemRemovedCallback));
        }

        [Test]
        public void DataCache_SetCache_With_Callback_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
            // Arrange
            DNNCacheDependency dep = this.CreateTestDependency(); // Dependency type or value doesn't matter
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
            CacheItemRemovedCallback callback = this.ItemRemovedCallback;

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, CacheItemPriority.Normal, this.ItemRemovedCallback);

            // Assert
            this.mockCache.Verify(
                cache => cache.Insert(this.GetDnnCacheKey(Constants.CACHEING_ValidKey), Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, CacheItemPriority.Normal, this.ItemRemovedCallback));
        }

        private DNNCacheDependency CreateTestDependency()
        {
            return new DNNCacheDependency("C:\\testdependency.txt");
        }

        private string GetDnnCacheKey(string cacheKey)
        {
            return string.Format("DNN_{0}", cacheKey);
        }

        private void ItemRemovedCallback(string key, object value, CacheItemRemovedReason removedReason)
        {
            // do nothing
        }
    }
}
