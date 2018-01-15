#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Data;
using System.Text;

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Tests.Utilities.Mocks;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Providers.Caching
{
    /// <summary>
    ///   Summary description for DataCacheTests
    /// </summary>
    [TestFixture]
    public class CBOTest
    {
        [SetUp]
        public void SetUp()
        {
            //Create a Container
            ComponentFactory.Container = new SimpleContainer();
            MockComponentProvider.CreateDataCacheProvider();
        }


        [Test]
        public void CBO_FillObject_int()
        {
            var cboTable = new DataTable("CBOTable");
            var colValue = 12;
            cboTable.Columns.Add("IntProp", typeof (int));
            cboTable.Rows.Add(colValue);

            //Assert.AreEqual(12, moq.Object["TestColumn"]);

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
            cboTable.Columns.Add("StringProp", typeof (String));
            cboTable.Rows.Add(colValue);

            //Assert.AreEqual(12, moq.Object["TestColumn"]);

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

            cboTable.Columns.Add("DateTimeProp", typeof (DateTime));
            cboTable.Rows.Add(colValue);

            //Assert.AreEqual(12, moq.Object["TestColumn"]);

            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());

            Assert.IsInstanceOf<IntPoco>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(colValue, result.DateTimeProp);
        }

        [Test]
        //DNNPRO-13404 - Object does not implement IConvertible 
        public void CBO_FillObject_binary()
        {
            var cboTable = new DataTable("CBOTable");
            var colValue = Encoding.ASCII.GetBytes("Hello This is test");

            cboTable.Columns.Add("ByteArrayProp", typeof (byte[]));
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

            cboTable.Columns.Add("ArrayProp", typeof (byte[]));
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

            cboTable.Columns.Add("BitProp", typeof (Boolean));
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

            cboTable.Columns.Add("DecimalProp", typeof (decimal));
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

            cboTable.Columns.Add("BitProp", typeof (int));
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

            cboTable.Columns.Add("BitProp", typeof (int));
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
        public Byte[] ByteArrayProp { get; set; }
        public Array ArrayProp { get; set; }
        public Boolean BitProp { get; set; }
        public decimal DecimalProp { get; set; }
    }
}