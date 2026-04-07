// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Entities.Claims
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Claims;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ClaimControllerTests
    {
        private Mock<DataProvider> mockDataProvider;

        [SetUp]
        public void SetUp()
        {
            this.mockDataProvider = MockComponentProvider.CreateDataProvider();
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
            ClaimController.ClearInstance();
        }

        [Test]
        public void AddClaim_Sets_Status_To_New()
        {
            // Arrange
            this.mockDataProvider
                .Setup(dp => dp.ExecuteScalar<int>(
                    "Claims_AddClaim",
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.Is<object>(s => (int)s == (int)ClaimStatus.New)))
                .Returns(1);

            var controller = new ClaimController(this.mockDataProvider.Object);
            var claim = new ClaimInfo
            {
                PortalId = 0,
                UserId = 1,
                Subject = "Test Claim",
                Description = "Test Description",
                Status = ClaimStatus.Closed, // should be overridden to New
            };

            // Act
            var claimId = controller.AddClaim(claim);

            // Assert
            Assert.That(claim.Status, Is.EqualTo(ClaimStatus.New));
            Assert.That(claimId, Is.EqualTo(1));
        }

        [Test]
        public void AddClaim_Throws_On_Null()
        {
            // Arrange
            var controller = new ClaimController(this.mockDataProvider.Object);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => controller.AddClaim(null));
        }

        [Test]
        public void UpdateClaim_Throws_On_Null()
        {
            // Arrange
            var controller = new ClaimController(this.mockDataProvider.Object);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => controller.UpdateClaim(null));
        }

        [Test]
        public void DeleteClaim_Calls_DataProvider()
        {
            // Arrange
            var controller = new ClaimController(this.mockDataProvider.Object);

            // Act
            controller.DeleteClaim(1);

            // Assert
            this.mockDataProvider.Verify(
                dp => dp.ExecuteNonQuery("Claims_DeleteClaim", 1),
                Times.Once);
        }

        [Test]
        [TestCase(ClaimStatus.New, ClaimStatus.Closed)]
        [TestCase(ClaimStatus.Closed, ClaimStatus.Reopened)]
        [TestCase(ClaimStatus.Reopened, ClaimStatus.Closed)]
        public void ValidateStatusTransition_Allows_Valid_Transitions(ClaimStatus from, ClaimStatus to)
        {
            // Act & Assert - should not throw
            Assert.DoesNotThrow(() => ClaimController.ValidateStatusTransition(from, to));
        }

        [Test]
        [TestCase(ClaimStatus.New, ClaimStatus.Reopened)]
        [TestCase(ClaimStatus.Closed, ClaimStatus.New)]
        [TestCase(ClaimStatus.Reopened, ClaimStatus.New)]
        public void ValidateStatusTransition_Rejects_Invalid_Transitions(ClaimStatus from, ClaimStatus to)
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidClaimStatusTransitionException>(
                () => ClaimController.ValidateStatusTransition(from, to));

            Assert.That(ex.CurrentStatus, Is.EqualTo(from));
            Assert.That(ex.NewStatus, Is.EqualTo(to));
        }

        [Test]
        [TestCase(ClaimStatus.New)]
        [TestCase(ClaimStatus.Closed)]
        [TestCase(ClaimStatus.Reopened)]
        public void ValidateStatusTransition_Allows_Same_Status(ClaimStatus status)
        {
            // Act & Assert - same status is a no-op, should not throw
            Assert.DoesNotThrow(() => ClaimController.ValidateStatusTransition(status, status));
        }

        [Test]
        public void ChangeStatus_Throws_When_Claim_Not_Found()
        {
            // Arrange
            this.mockDataProvider
                .Setup(dp => dp.ExecuteReader("Claims_GetClaim", 999))
                .Returns(this.CreateEmptyClaimReader());

            var controller = new ClaimController(this.mockDataProvider.Object);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => controller.ChangeStatus(999, ClaimStatus.Closed));
        }

        [Test]
        public void ChangeStatus_Throws_On_Invalid_Transition()
        {
            // Arrange
            this.mockDataProvider
                .Setup(dp => dp.ExecuteReader("Claims_GetClaim", 1))
                .Returns(this.CreateClaimReader(1, ClaimStatus.New));

            var controller = new ClaimController(this.mockDataProvider.Object);

            // Act & Assert
            Assert.Throws<InvalidClaimStatusTransitionException>(
                () => controller.ChangeStatus(1, ClaimStatus.Reopened));
        }

        [Test]
        public void ChangeStatus_Succeeds_On_Valid_Transition()
        {
            // Arrange
            this.mockDataProvider
                .Setup(dp => dp.ExecuteReader("Claims_GetClaim", 1))
                .Returns(this.CreateClaimReader(1, ClaimStatus.New));

            var controller = new ClaimController(this.mockDataProvider.Object);

            // Act
            controller.ChangeStatus(1, ClaimStatus.Closed);

            // Assert
            this.mockDataProvider.Verify(
                dp => dp.ExecuteNonQuery("Claims_ChangeStatus", 1, (int)ClaimStatus.Closed),
                Times.Once);
        }

        [Test]
        public void NewClaim_Has_Default_Values()
        {
            // Act
            var claim = new ClaimInfo();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(claim.ClaimId, Is.EqualTo(Null.NullInteger));
                Assert.That(claim.PortalId, Is.EqualTo(Null.NullInteger));
                Assert.That(claim.UserId, Is.EqualTo(Null.NullInteger));
                Assert.That(claim.Status, Is.EqualTo(ClaimStatus.New));
            }
        }

        [Test]
        public void ClaimInfo_KeyID_Maps_To_ClaimId()
        {
            // Arrange
            var claim = new ClaimInfo();

            // Act
            claim.KeyID = 42;

            // Assert
            Assert.That(claim.ClaimId, Is.EqualTo(42));
            Assert.That(claim.KeyID, Is.EqualTo(42));
        }

        private IDataReader CreateEmptyClaimReader()
        {
            var table = this.CreateClaimDataTable();
            return table.CreateDataReader();
        }

        private IDataReader CreateClaimReader(int claimId, ClaimStatus status)
        {
            var table = this.CreateClaimDataTable();
            var row = table.NewRow();
            row["ClaimId"] = claimId;
            row["PortalId"] = 0;
            row["UserId"] = 1;
            row["Subject"] = "Test";
            row["Description"] = "Test Description";
            row["Status"] = (int)status;
            row["CreatedByUserID"] = -1;
            row["CreatedOnDate"] = DateTime.Now;
            row["LastModifiedByUserID"] = -1;
            row["LastModifiedOnDate"] = DateTime.Now;
            table.Rows.Add(row);
            return table.CreateDataReader();
        }

        private DataTable CreateClaimDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("ClaimId", typeof(int));
            table.Columns.Add("PortalId", typeof(int));
            table.Columns.Add("UserId", typeof(int));
            table.Columns.Add("Subject", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Status", typeof(int));
            table.Columns.Add("CreatedByUserID", typeof(int));
            table.Columns.Add("CreatedOnDate", typeof(DateTime));
            table.Columns.Add("LastModifiedByUserID", typeof(int));
            table.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            return table;
        }
    }
}
