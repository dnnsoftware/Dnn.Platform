// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using System;
    using System.Collections.Generic;
    using System.Web.Caching;

    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Data.Fakes;
    using DotNetNuke.Tests.Data.Models;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using Moq.Protected;
    using NUnit.Framework;

    [TestFixture]
    public class RepositoryBaseTests
    {
        // ReSharper disable InconsistentNaming
        [SetUp]
        public void SetUp()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_CacheArgs_Null_If_Not_Cacheable()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.IsNull(Util.GetPrivateMember<RepositoryBase<Dog>, CacheItemArgs>(baseRepo, "CacheArgs"));
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_CacheArgs_If_Cacheable()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<CacheableDog>();

            // Assert
            var baseRepo = repo as RepositoryBase<CacheableDog>;
            Assert.IsNotNull(Util.GetPrivateMember<RepositoryBase<CacheableDog>, CacheItemArgs>(baseRepo, "CacheArgs"));
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_Valid_CacheArgs_If_Cacheable()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<CacheableDog>();

            // Assert
            var cacheArgs = Util.GetPrivateMember<FakeRepository<CacheableDog>, CacheItemArgs>(repo, "CacheArgs");
            Assert.AreEqual(Constants.CACHE_DogsKey, cacheArgs.CacheKey);
            Assert.AreEqual(Constants.CACHE_Priority, cacheArgs.CachePriority);
            Assert.AreEqual(Constants.CACHE_TimeOut, cacheArgs.CacheTimeOut);
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_IsCacheable_False_If_Not_Cacheable()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.IsFalse(Util.GetPrivateMember<RepositoryBase<Dog>, bool>(baseRepo, "IsCacheable"));
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_IsCacheable_True_If_Cacheable()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<CacheableDog>();

            // Assert
            var baseRepo = repo as RepositoryBase<CacheableDog>;
            Assert.IsTrue(Util.GetPrivateMember<RepositoryBase<CacheableDog>, bool>(baseRepo, "IsCacheable"));
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_IsScoped_False_If_Not_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.IsFalse(Util.GetPrivateMember<RepositoryBase<Dog>, bool>(baseRepo, "IsScoped"));
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_IsScoped_False_If_Cacheable_And_Not_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<CacheableDog>();

            // Assert
            var baseRepo = repo as RepositoryBase<CacheableDog>;
            Assert.IsFalse(Util.GetPrivateMember<RepositoryBase<CacheableDog>, bool>(baseRepo, "IsScoped"));
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_IsScoped_True_If_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Cat>();

            // Assert
            var baseRepo = repo as RepositoryBase<Cat>;
            Assert.IsTrue(Util.GetPrivateMember<RepositoryBase<Cat>, bool>(baseRepo, "IsScoped"));
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_IsScoped_True_If_Cacheable_And_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<CacheableCat>();

            // Assert
            var baseRepo = repo as RepositoryBase<CacheableCat>;
            Assert.IsTrue(Util.GetPrivateMember<RepositoryBase<CacheableCat>, bool>(baseRepo, "IsScoped"));
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_Scope_Empty_If_Not_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.AreEqual(string.Empty, Util.GetPrivateMember<RepositoryBase<Dog>, string>(baseRepo, "Scope"));
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_Scope_Empty_If_Cacheable_And_Not_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<CacheableDog>();

            // Assert
            var baseRepo = repo as RepositoryBase<CacheableDog>;
            Assert.AreEqual(string.Empty, Util.GetPrivateMember<RepositoryBase<CacheableDog>, string>(baseRepo, "Scope"));
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_Scope_If_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Cat>();

            // Assert
            var baseRepo = repo as RepositoryBase<Cat>;
            Assert.AreEqual(Constants.CACHE_ScopeModule, Util.GetPrivateMember<RepositoryBase<Cat>, string>(baseRepo, "Scope"));
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_Scope_If_Cacheable_And_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<CacheableCat>();

            // Assert
            var baseRepo = repo as RepositoryBase<CacheableCat>;
            Assert.AreEqual(Constants.CACHE_ScopeModule, Util.GetPrivateMember<RepositoryBase<CacheableCat>, string>(baseRepo, "Scope"));
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_CacheArgs_Null_If_Not_Cacheable()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(string.Empty);

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.IsNull(Util.GetPrivateMember<RepositoryBase<Dog>, CacheItemArgs>(baseRepo, "CacheArgs"));
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_CacheArgs_If_Cacheable()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(Constants.CACHE_DogsKey);

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.IsNotNull(Util.GetPrivateMember<RepositoryBase<Dog>, CacheItemArgs>(baseRepo, "CacheArgs"));
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_Valid_CacheArgs_If_Cacheable()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(Constants.CACHE_DogsKey, Constants.CACHE_TimeOut, Constants.CACHE_Priority);

            // Assert
            var cacheArgs = Util.GetPrivateMember<FakeRepository<Dog>, CacheItemArgs>(repo, "CacheArgs");
            Assert.AreEqual(Constants.CACHE_DogsKey, cacheArgs.CacheKey);
            Assert.AreEqual(Constants.CACHE_Priority, cacheArgs.CachePriority);
            Assert.AreEqual(Constants.CACHE_TimeOut, cacheArgs.CacheTimeOut);
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_IsCacheable_False_If_Not_Cacheable()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.IsFalse(Util.GetPrivateMember<RepositoryBase<Dog>, bool>(baseRepo, "IsCacheable"));
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_IsCacheable_True_If_Cacheable()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(Constants.CACHE_DogsKey);

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.IsTrue(Util.GetPrivateMember<RepositoryBase<Dog>, bool>(baseRepo, "IsCacheable"));
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_IsScoped_False_If_Not_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(string.Empty);

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.IsFalse(Util.GetPrivateMember<RepositoryBase<Dog>, bool>(baseRepo, "IsScoped"));
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_IsScoped_False_If_Cacheable_And_Not_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(Constants.CACHE_DogsKey, Constants.CACHE_TimeOut, Constants.CACHE_Priority);

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.IsFalse(Util.GetPrivateMember<RepositoryBase<Dog>, bool>(baseRepo, "IsScoped"));
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_IsScoped_True_If_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Cat>();
            repo.Initialize(string.Empty, 20, CacheItemPriority.Default, Constants.CACHE_ScopeModule);

            // Assert
            var baseRepo = repo as RepositoryBase<Cat>;
            Assert.IsTrue(Util.GetPrivateMember<RepositoryBase<Cat>, bool>(baseRepo, "IsScoped"));
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_IsScoped_True_If_Cacheable_And_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Cat>();
            repo.Initialize(Constants.CACHE_CatsKey, Constants.CACHE_TimeOut, Constants.CACHE_Priority, Constants.CACHE_ScopeModule);

            // Assert
            var baseRepo = repo as RepositoryBase<Cat>;
            Assert.IsTrue(Util.GetPrivateMember<RepositoryBase<Cat>, bool>(baseRepo, "IsScoped"));
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_Scope_Empty_If_Not_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(string.Empty);

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.AreEqual(string.Empty, Util.GetPrivateMember<RepositoryBase<Dog>, string>(baseRepo, "Scope"));
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_Scope_Empty_If_Cacheable_And_Not_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(Constants.CACHE_DogsKey, Constants.CACHE_TimeOut, Constants.CACHE_Priority);

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.AreEqual(string.Empty, Util.GetPrivateMember<RepositoryBase<Dog>, string>(baseRepo, "Scope"));
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_Scope_If_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Cat>();
            repo.Initialize(string.Empty, 20, CacheItemPriority.Default, Constants.CACHE_ScopeModule);

            // Assert
            var baseRepo = repo as RepositoryBase<Cat>;
            Assert.AreEqual(Constants.CACHE_ScopeModule, Util.GetPrivateMember<RepositoryBase<Cat>, string>(baseRepo, "Scope"));
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_Scope_If_Cacheable_And_Scoped()
        {
            // Arrange

            // Act
            var repo = new FakeRepository<Cat>();
            repo.Initialize(Constants.CACHE_CatsKey, Constants.CACHE_TimeOut, Constants.CACHE_Priority, Constants.CACHE_ScopeModule);

            // Assert
            var baseRepo = repo as RepositoryBase<Cat>;
            Assert.AreEqual(Constants.CACHE_ScopeModule, Util.GetPrivateMember<RepositoryBase<Cat>, string>(baseRepo, "Scope"));
        }

        [Test]
        public void RepositoryBase_Delete_Clears_Cache_If_Cacheable()
        {
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(Constants.CACHE_DogsKey);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableDog>());

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

            // Act
            mockRepository.Object.Delete(new CacheableDog());

            // Assert
            mockCache.Verify(c => c.Remove(cacheKey), Times.Once());
        }

        [Test]
        public void RepositoryBase_Delete_Clears_Cache_If_Cacheable_And_Scoped()
        {
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

            // Act
            mockRepository.Object.Delete(new CacheableCat { ModuleId = Constants.MODULE_ValidId });

            // Assert
            mockCache.Verify(c => c.Remove(cacheKey), Times.Once());
        }

        [Test]
        public void RepositoryBase_Delete_Does_Not_Clear_Cache_If_Not_Cacheable()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

            var mockRepository = new Mock<RepositoryBase<Dog>>();

            // Act
            mockRepository.Object.Delete(new Dog());

            // Assert
            mockCache.Verify(c => c.Remove(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void RepositoryBase_Delete_Calls_DeleteInternal()
        {
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();
            mockRepository.Protected().Setup("DeleteInternal", ItExpr.IsAny<Dog>());

            // Act
            mockRepository.Object.Delete(new Dog());

            // Assert
            mockRepository.Protected().Verify("DeleteInternal", Times.Once(), ItExpr.IsAny<Dog>());
        }

        [Test]
        public void RepositoryBase_Get_Checks_Cache_If_Cacheable()
        {
            // Arrange
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey))).Returns(new List<CacheableDog>());

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

            // Act
            var list = mockRepository.Object.Get();

            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)));
        }

        [Test]
        public void RepositoryBase_Get_Does_Not_Check_Cache_If_Not_Cacheable()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

            var mockRepository = new Mock<RepositoryBase<Dog>>();

            // Act
            var list = mockRepository.Object.Get();

            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)), Times.Never());
        }

        [Test]
        public void RepositoryBase_Get_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

            // Act
            var list = mockRepository.Object.Get();

            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_CatsKey)), Times.Never());
        }

        [Test]
        public void RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_And_Cache_Expired()
        {
            // Arrange
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey))).Returns(null);

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();
            mockRepository.Protected().Setup("GetInternal");

            // Act
            var list = mockRepository.Object.Get();

            // Assert
            mockRepository.Protected().Verify("GetInternal", Times.Once());
        }

        [Test]
        public void RepositoryBase_Get_Calls_GetAllInternal_If_Not_Cacheable()
        {
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();
            mockRepository.Protected().Setup("GetInternal");

            // Act
            var list = mockRepository.Object.Get();

            // Assert
            mockRepository.Protected().Verify("GetInternal", Times.Once());
        }

        [Test]
        public void RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_But_Not_Scoped()
        {
            // Arrange
            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();
            mockRepository.Protected().Setup("GetInternal");

            // Act
            var list = mockRepository.Object.Get();

            // Assert
            mockRepository.Protected().Verify("GetInternal", Times.Once());
        }

        [Test]
        public void RepositoryBase_Get_Does_Not_Call_GetAllInternal_If_Cacheable_And_Cache_Valid()
        {
            // Arrange
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey))).Returns(new List<CacheableDog>());

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();
            mockRepository.Protected().Setup("GetInternal");

            // Act
            var list = mockRepository.Object.Get();

            // Assert
            mockRepository.Protected().Verify("GetInternal", Times.Never());
        }

        [Test]
        public void RepositoryBase_Get_Overload_Checks_Cache_If_Cacheable_And_Scoped()
        {
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

            // Act
            var list = mockRepository.Object.Get<int>(Constants.MODULE_ValidId);

            // Assert
            mockCache.Verify(c => c.GetItem(cacheKey));
        }

        [Test]
        public void RepositoryBase_Get_Overload_Throws_If_Not_Cacheable()
        {
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();

            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.Get<int>(Constants.MODULE_ValidId));
        }

        [Test]
        public void RepositoryBase_Get_Overload_Throws_If_Not_Scoped()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

            var mockRepository = new Mock<RepositoryBase<Dog>>();

            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.Get<int>(Constants.MODULE_ValidId));
        }

        [Test]
        public void RepositoryBase_Get_Overload_Throws_If_Cacheable_But_Not_Scoped()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.Get<int>(Constants.MODULE_ValidId));
        }

        [Test]
        public void RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Not_Cacheable_And_Is_Scoped()
        {
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Cat>>();
            mockRepository.Protected().Setup<IEnumerable<Cat>>("GetByScopeInternal", ItExpr.IsAny<object>());

            // Act
            var list = mockRepository.Object.Get<int>(Constants.MODULE_ValidId);

            // Assert
            mockRepository.Protected().Verify<IEnumerable<Cat>>("GetByScopeInternal", Times.Once(), ItExpr.IsAny<object>());
        }

        [Test]
        public void RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired()
        {
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(cacheKey))).Returns(null);

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();
            mockRepository.Protected().Setup<IEnumerable<CacheableCat>>("GetByScopeInternal", ItExpr.IsAny<object>());

            // Act
            var list = mockRepository.Object.Get<int>(Constants.MODULE_ValidId);

            // Assert
            mockRepository.Protected().Verify<IEnumerable<CacheableCat>>("GetByScopeInternal", Times.Once(), ItExpr.IsAny<object>());
        }

        [Test]
        public void RepositoryBase_Get_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid()
        {
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();
            mockRepository.Protected().Setup<IEnumerable<CacheableCat>>("GetByScopeInternal", ItExpr.IsAny<object>());

            // Act
            var list = mockRepository.Object.Get<int>(Constants.MODULE_ValidId);

            // Assert
            mockRepository.Protected().Verify<IEnumerable<CacheableCat>>("GetByScopeInternal", Times.Never(), ItExpr.IsAny<object>());
        }

        [Test]
        public void RepositoryBase_GetById_Checks_Cache_If_Cacheable()
        {
            // Arrange
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey))).Returns(new List<CacheableDog>());

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

            // Act
            var dog = mockRepository.Object.GetById(Constants.PETAPOCO_ValidDogId);

            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)));
        }

        [Test]
        public void RepositoryBase_GetById_Does_Not_Check_Cache_If_Not_Cacheable()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

            var mockRepository = new Mock<RepositoryBase<Dog>>();

            // Act
            var dog = mockRepository.Object.GetById(Constants.PETAPOCO_ValidDogId);

            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)), Times.Never());
        }

        [Test]
        public void RepositoryBase_GetById_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

            // Act
            var cat = mockRepository.Object.GetById(Constants.PETAPOCO_ValidCatId);

            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)), Times.Never());
        }

        [Test]
        public void RepositoryBase_GetById_Calls_GetByIdInternal_If_Not_Cacheable()
        {
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();
            mockRepository.Protected().Setup<Dog>("GetByIdInternal", ItExpr.IsAny<object>());

            // Act
            var dog = mockRepository.Object.GetById(Constants.PETAPOCO_ValidDogId);

            // Assert
            mockRepository.Protected().Verify<Dog>("GetByIdInternal", Times.Once(), ItExpr.IsAny<object>());
        }

        [Test]
        public void RepositoryBase_GetById_Calls_GetByIdInternal_If_Cacheable_But_Not_Scoped()
        {
            // Arrange
            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();
            mockRepository.Protected().Setup<CacheableCat>("GetByIdInternal", ItExpr.IsAny<object>());

            // Act
            var cat = mockRepository.Object.GetById(Constants.PETAPOCO_ValidCatId);

            // Assert
            mockRepository.Protected().Verify<CacheableCat>("GetByIdInternal", Times.Once(), ItExpr.IsAny<object>());
        }

        [Test]
        public void RepositoryBase_GetById_Does_Not_Call_GetByIdInternal_If_Cacheable_And_Cache_Valid()
        {
            // Arrange
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey))).Returns(new List<CacheableDog>());

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();
            mockRepository.Protected().Setup<CacheableDog>("GetByIdInternal", ItExpr.IsAny<object>());

            // Act
            var dog = mockRepository.Object.GetById(Constants.PETAPOCO_ValidDogId);

            // Assert
            mockRepository.Protected().Verify<CacheableDog>("GetByIdInternal", Times.Never(), ItExpr.IsAny<object>());
        }

        [Test]
        public void RepositoryBase_GetById_Overload_Checks_Cache_If_Cacheable_And_Scoped()
        {
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

            // Act
            var cat = mockRepository.Object.GetById(Constants.PETAPOCO_ValidCatId, Constants.MODULE_ValidId);

            // Assert
            mockCache.Verify(c => c.GetItem(cacheKey));
        }

        [Test]
        public void RepositoryBase_GetById_Overload_Throws_If_Not_Cacheable()
        {
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();

            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.GetById(Constants.PETAPOCO_ValidDogId, Constants.MODULE_ValidId));
        }

        [Test]
        public void RepositoryBase_GetById_Overload_Throws__If_Cacheable_But_Not_Scoped()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.GetById(Constants.PETAPOCO_ValidDogId, Constants.MODULE_ValidId));
        }

        [Test]
        public void RepositoryBase_GetPage_Checks_Cache_If_Cacheable()
        {
            // Arrange
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey))).Returns(new List<CacheableDog>());

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

            // Act
            var dogs = mockRepository.Object.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)));
        }

        [Test]
        public void RepositoryBase_GetPage_Does_Not_Check_Cache_If_Not_Cacheable()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

            var mockRepository = new Mock<RepositoryBase<Dog>>();

            // Act
            var dogs = mockRepository.Object.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)), Times.Never());
        }

        [Test]
        public void RepositoryBase_GetPage_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

            // Act
            var cats = mockRepository.Object.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)), Times.Never());
        }

        [Test]
        public void RepositoryBase_GetPage_Calls_GetAllByPageInternal_If_Not_Cacheable()
        {
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();
            mockRepository.Protected().Setup<IPagedList<Dog>>("GetPageInternal", ItExpr.IsAny<int>(), ItExpr.IsAny<int>());

            // Act
            var dogs = mockRepository.Object.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

            // Assert
            mockRepository.Protected().Verify<IPagedList<Dog>>("GetPageInternal", Times.Once(), ItExpr.IsAny<int>(), ItExpr.IsAny<int>());
        }

        [Test]
        public void RepositoryBase_GetPage_Calls_GetAllByPageInternal_If_Cacheable_But_Not_Scoped()
        {
            // Arrange
            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();
            mockRepository.Protected().Setup<IPagedList<CacheableCat>>("GetPageInternal", ItExpr.IsAny<int>(), ItExpr.IsAny<int>());

            // Act
            var cats = mockRepository.Object.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

            // Assert
            mockRepository.Protected().Verify<IPagedList<CacheableCat>>("GetPageInternal", Times.Once(), ItExpr.IsAny<int>(), ItExpr.IsAny<int>());
        }

        [Test]
        public void RepositoryBase_GetPage_Does_Not_Call_GetAllByPageInternal_If_Cacheable_And_Cache_Valid()
        {
            // Arrange
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)))
                        .Returns(new List<CacheableDog>() { new CacheableDog() });

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();
            mockRepository.Protected().Setup<IPagedList<CacheableDog>>("GetPageInternal", ItExpr.IsAny<int>(), ItExpr.IsAny<int>());

            // Act
            var dogs = mockRepository.Object.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

            // Assert
            mockRepository.Protected().Verify<IPagedList<CacheableDog>>("GetPageInternal", Times.Never(), ItExpr.IsAny<int>(), ItExpr.IsAny<int>());
        }

        [Test]
        public void RepositoryBase_GetPage_Overload_Checks_Cache_If_Cacheable_And_Scoped()
        {
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

            // Act
            var cats = mockRepository.Object.GetPage<int>(Constants.MODULE_ValidId, Constants.PAGE_First, Constants.PAGE_RecordCount);

            // Assert
            mockCache.Verify(c => c.GetItem(cacheKey));
        }

        [Test]
        public void RepositoryBase_GetPage_Overload_Throws_If_Not_Cacheable()
        {
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();

            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.GetPage<int>(Constants.MODULE_ValidId, Constants.PAGE_First, Constants.PAGE_RecordCount));
        }

        [Test]
        public void RepositoryBase_GetPage_Overload_Throws_If_Not_Scoped()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

            var mockRepository = new Mock<RepositoryBase<Dog>>();

            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.GetPage<int>(Constants.MODULE_ValidId, Constants.PAGE_First, Constants.PAGE_RecordCount));
        }

        [Test]
        public void RepositoryBase_GetPage_Overload_Throws_If_Cacheable_But_Not_Scoped()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.GetPage<int>(Constants.MODULE_ValidId, Constants.PAGE_First, Constants.PAGE_RecordCount));
        }

        [Test]
        public void RepositoryBase_GetPage_Overload_Calls_GetAllByScopeAndPageInternal_If_Not_Cacheable_And_Is_Scoped()
        {
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Cat>>();
            mockRepository.Protected().Setup<IEnumerable<Cat>>("GetPageByScopeInternal", ItExpr.IsAny<object>(), ItExpr.IsAny<int>(), ItExpr.IsAny<int>());

            // Act
            var cats = mockRepository.Object.GetPage<int>(Constants.MODULE_ValidId, Constants.PAGE_First, Constants.PAGE_RecordCount);

            // Assert
            mockRepository.Protected().Verify<IEnumerable<Cat>>("GetPageByScopeInternal", Times.Once(), ItExpr.IsAny<object>(), ItExpr.IsAny<int>(), ItExpr.IsAny<int>());
        }

        [Test]
        public void RepositoryBase_GetPage_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired()
        {
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("0");

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(cacheKey))).Returns(null);

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();
            mockRepository.Protected().Setup<IEnumerable<CacheableCat>>("GetByScopeInternal", ItExpr.IsAny<object>())
                                    .Returns(new List<CacheableCat>());

            var mockData = MockComponentProvider.CreateDataProvider();
            mockData.Setup(d => d.GetProviderPath()).Returns(string.Empty);

            // Act
            var cats = mockRepository.Object.GetPage<int>(Constants.MODULE_ValidId, Constants.PAGE_First, Constants.PAGE_RecordCount);

            // Assert
            mockRepository.Protected().Verify<IEnumerable<CacheableCat>>("GetByScopeInternal", Times.Once(), ItExpr.IsAny<object>());
        }

        [Test]
        public void RepositoryBase_GetPage_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid()
        {
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();
            mockRepository.Protected().Setup<IEnumerable<CacheableCat>>("GetByScopeInternal", ItExpr.IsAny<object>());

            // Act
            var cats = mockRepository.Object.GetPage<int>(Constants.MODULE_ValidId, Constants.PAGE_First, Constants.PAGE_RecordCount);

            // Assert
            mockRepository.Protected().Verify<IEnumerable<CacheableCat>>("GetByScopeInternal", Times.Never(), ItExpr.IsAny<object>());
        }

        [Test]
        public void RepositoryBase_Insert_Clears_Cache_If_Cacheable()
        {
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(Constants.CACHE_DogsKey);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableDog>());

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

            // Act
            mockRepository.Object.Insert(new CacheableDog());

            // Assert
            mockCache.Verify(c => c.Remove(cacheKey), Times.Once());
        }

        [Test]
        public void RepositoryBase_Insert_Clears_Cache_If_Cacheable_And_Scoped()
        {
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

            // Act
            mockRepository.Object.Insert(new CacheableCat { ModuleId = Constants.MODULE_ValidId });

            // Assert
            mockCache.Verify(c => c.Remove(cacheKey), Times.Once());
        }

        [Test]
        public void RepositoryBase_Insert_Does_Not_Clear_Cache_If_Not_Cacheable()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

            var mockRepository = new Mock<RepositoryBase<Dog>>();

            // Act
            mockRepository.Object.Insert(new Dog());

            // Assert
            mockCache.Verify(c => c.Remove(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void RepositoryBase_Insert_Calls_InsertInternal()
        {
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();
            mockRepository.Protected().Setup("InsertInternal", ItExpr.IsAny<Dog>());

            // Act
            mockRepository.Object.Insert(new Dog());

            // Assert
            mockRepository.Protected().Verify("InsertInternal", Times.Once(), ItExpr.IsAny<Dog>());
        }

        [Test]
        public void RepositoryBase_Update_Clears_Cache_If_Cacheable()
        {
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(Constants.CACHE_DogsKey);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableDog>());

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

            // Act
            mockRepository.Object.Update(new CacheableDog());

            // Assert
            mockCache.Verify(c => c.Remove(cacheKey), Times.Once());
        }

        [Test]
        public void RepositoryBase_Update_Clears_Cache_If_Cacheable_And_Scoped()
        {
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

            // Act
            mockRepository.Object.Update(new CacheableCat { ModuleId = Constants.MODULE_ValidId });

            // Assert
            mockCache.Verify(c => c.Remove(cacheKey), Times.Once());
        }

        [Test]
        public void RepositoryBase_Update_Does_Not_Clear_Cache_If_Not_Cacheable()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

            var mockRepository = new Mock<RepositoryBase<Dog>>();

            // Act
            mockRepository.Object.Update(new Dog());

            // Assert
            mockCache.Verify(c => c.Remove(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void RepositoryBase_Update_Calls_UpdateInternal()
        {
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();
            mockRepository.Protected().Setup("UpdateInternal", ItExpr.IsAny<Dog>());

            // Act
            mockRepository.Object.Update(new Dog());

            // Assert
            mockRepository.Protected().Verify("UpdateInternal", Times.Once(), ItExpr.IsAny<Dog>());
        }

        // ReSharper restore InconsistentNaming
    }
}
