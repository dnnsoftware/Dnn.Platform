// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Caching
{
    using System;
    using System.Data;
    using System.Text;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Tests.Utilities.Mocks;
    using NUnit.Framework;

    /// <summary>
    ///   Summary description for DataCacheTests.
    /// </summary>
    [TestFixture]
    public class CBOTest
    {
        [SetUp]
        public void SetUp()
        {
            // Create a Container
            ComponentFactory.Container = new SimpleContainer();
            MockComponentProvider.CreateDataCacheProvider();
        }

        [Test]
        public void CBO_FillObject_int()
        {
            var cboTable = new DataTable("CBOTable");
            var colValue = 12;
            cboTable.Columns.Add("IntProp", typeof(int));
            cboTable.Rows.Add(colValue);

            // Assert.AreEqual(12, moq.Object["TestColumn"]);
            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());

            Assert.IsInstanceOf<IntPoco>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(colValue, result.IntProp);
        }

        [Test]
        public void CBO_FillObject_string()
        {
            var cboTable = new DataTable("CBOTable");
            var colValue = Guid.NewGuid().ToString();
            cboTable.Columns.Add("StringProp", typeof(string));
            cboTable.Rows.Add(colValue);

            // Assert.AreEqual(12, moq.Object["TestColumn"]);
            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());

            Assert.IsInstanceOf<IntPoco>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(colValue, result.StringProp);
        }

        [Test]
        public void CBO_FillObject_datetime()
        {
            var cboTable = new DataTable("CBOTable");
            var colValue = new DateTime(2010, 12, 11, 10, 9, 8);

            cboTable.Columns.Add("DateTimeProp", typeof(DateTime));
            cboTable.Rows.Add(colValue);

            // Assert.AreEqual(12, moq.Object["TestColumn"]);
            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());

            Assert.IsInstanceOf<IntPoco>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(colValue, result.DateTimeProp);
        }

        [Test]

        // DNNPRO-13404 - Object does not implement IConvertible
        public void CBO_FillObject_binary()
        {
            var cboTable = new DataTable("CBOTable");
            var colValue = Encoding.ASCII.GetBytes("Hello This is test");

            cboTable.Columns.Add("ByteArrayProp", typeof(byte[]));
            cboTable.Rows.Add(colValue);

            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());

            Assert.IsInstanceOf<IntPoco>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(colValue, result.ByteArrayProp);
        }

        [Test]
        public void CBO_FillObject_binary_to_Array()
        {
            var cboTable = new DataTable("CBOTable");
            var colValue = Encoding.ASCII.GetBytes("Hello This is test");

            cboTable.Columns.Add("ArrayProp", typeof(byte[]));
            cboTable.Rows.Add(colValue);

            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());

            Assert.IsInstanceOf<IntPoco>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(colValue, result.ArrayProp);
        }

        [Test]
        public void CBO_FillObject_bit()
        {
            var cboTable = new DataTable("CBOTable");
            var colValue = true;

            cboTable.Columns.Add("BitProp", typeof(bool));
            cboTable.Rows.Add(colValue);

            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());

            Assert.IsInstanceOf<IntPoco>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(colValue, result.BitProp);
        }

        [Test]
        public void CBO_FillObject_decimal()
        {
            var cboTable = new DataTable("CBOTable");
            decimal colValue = 12.99m;

            cboTable.Columns.Add("DecimalProp", typeof(decimal));
            cboTable.Rows.Add(colValue);

            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());

            Assert.IsInstanceOf<IntPoco>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(colValue, result.DecimalProp);
        }

        [Test]
        public void CBO_FillObject_int_to_boolean_true()
        {
            var cboTable = new DataTable("CBOTable");
            decimal colValue = 1;

            cboTable.Columns.Add("BitProp", typeof(int));
            cboTable.Rows.Add(colValue);

            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());

            Assert.IsInstanceOf<IntPoco>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(true, result.BitProp);
        }

        [Test]
        public void CBO_FillObject_int_to_boolean_false()
        {
            var cboTable = new DataTable("CBOTable");
            decimal colValue = 0;

            cboTable.Columns.Add("BitProp", typeof(int));
            cboTable.Rows.Add(colValue);

            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());

            Assert.IsInstanceOf<IntPoco>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.BitProp);
        }
    }

    public class IntPoco
    {
        public int IntProp { get; set; }

        public string StringProp { get; set; }

        public DateTime DateTimeProp { get; set; }

        public byte[] ByteArrayProp { get; set; }

        public Array ArrayProp { get; set; }

        public bool BitProp { get; set; }

        public decimal DecimalProp { get; set; }
    }
}
